using System.Text;
using FluentValidation;
using th_mm_pos.application.DTOs;
using th_mm_pos.application.Interfaces;
using th_mm_pos.application.Validators;
using th_mm_pos.domain.Entities;
using th_mm_pos.domain.Interfaces;

namespace th_mm_pos.application.Services;

public class SalesService(
    IUnitOfWork unitOfWork
) : ISalesService
{
    private readonly TransactionValidator _validator = new();
    private const decimal TaxRate = 0.10m; // 10% tax

    public async Task<TransactionDto> CreateTransactionAsync(TransactionDto transactionDto, int cashierId)
    {
        // Validate
        var validationResult = await _validator.ValidateAsync(transactionDto);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // Begin transaction
        await unitOfWork.BeginTransactionAsync();

        try
        {
            // Check inventory availability for all items
            foreach (var item in transactionDto.Items)
            {
                var product = await unitOfWork.Products.GetByIdAsync(item.ProductId);
                if (product == null)
                {
                    throw new Exception($"Product with ID {item.ProductId} not found");
                }

                if (product.Quantity < item.Quantity)
                {
                    throw new Exception(
                        $"Insufficient inventory for product '{product.Name}'. Available: {product.Quantity}, Requested: {item.Quantity}");
                }
            }

            // Calculate totals
            decimal subtotal = transactionDto.Items.Sum(i => i.LineTotal);
            decimal tax = subtotal * TaxRate;
            decimal total = subtotal + tax - transactionDto.Discount;

            // Create transaction
            var transaction = new Transaction
            {
                TransactionDate = DateTime.UtcNow,
                Subtotal = subtotal,
                Tax = tax,
                Discount = transactionDto.Discount,
                Total = total,
                PaymentMethod = transactionDto.PaymentMethod,
                CashierId = cashierId,
                IsVoided = false
            };

            await unitOfWork.Transactions.AddAsync(transaction);
            await unitOfWork.SaveChangesAsync();

            // Create transaction items and decrement inventory
            foreach (var itemDto in transactionDto.Items)
            {
                var product = await unitOfWork.Products.GetByIdAsync(itemDto.ProductId);

                // Decrement inventory
                product?.Quantity -= itemDto.Quantity;
                product?.ModifiedAt = DateTime.UtcNow;
                if (product != null) await unitOfWork.Products.UpdateAsync(product);
            }

            await unitOfWork.SaveChangesAsync();

            // Log audit entry
            var auditLog = new AuditLog
            {
                UserId = cashierId,
                Action = "CREATE_TRANSACTION",
                EntityType = "Transaction",
                EntityId = transaction.Id,
                NewValue = $"Transaction created: Total={total:C}, Items={transactionDto.Items.Count}",
                Timestamp = DateTime.UtcNow
            };
            await unitOfWork.AuditLogs.AddAsync(auditLog);
            await unitOfWork.SaveChangesAsync();

            await unitOfWork.CommitTransactionAsync();

            return MapToDto(transaction);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<TransactionDto> VoidTransactionAsync(int transactionId, int userId)
    {
        var transaction = await unitOfWork.Transactions.GetByIdAsync(transactionId);
        if (transaction == null)
        {
            throw new Exception("Transaction not found");
        }

        if (transaction.IsVoided)
        {
            throw new Exception("Transaction is already voided");
        }

        await unitOfWork.BeginTransactionAsync();

        try
        {
            // Mark transaction as voided
            transaction.IsVoided = true;
            await unitOfWork.Transactions.UpdateAsync(transaction);

            // Restore inventory quantities
            foreach (var item in transaction.Items)
            {
                var product = await unitOfWork.Products.GetByIdAsync(item.ProductId);
                if (product != null)
                {
                    product.Quantity += item.Quantity;
                    product.ModifiedAt = DateTime.UtcNow;
                    await unitOfWork.Products.UpdateAsync(product);
                }
            }

            await unitOfWork.SaveChangesAsync();

            // Log audit entry
            var auditLog = new AuditLog
            {
                UserId = userId,
                Action = "VOID_TRANSACTION",
                EntityType = "Transaction",
                EntityId = transaction.Id,
                OldValue = $"Transaction voided: ID={transactionId}",
                Timestamp = DateTime.UtcNow
            };
            await unitOfWork.AuditLogs.AddAsync(auditLog);
            await unitOfWork.SaveChangesAsync();

            await unitOfWork.CommitTransactionAsync();

            return MapToDto(transaction);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<TransactionDto> ProcessReturnAsync(int originalTransactionId, List<int> itemIds, int userId)
    {
        var originalTransaction = await unitOfWork.Transactions.GetByIdAsync(originalTransactionId);
        if (originalTransaction == null)
        {
            throw new Exception("Original transaction not found");
        }

        await unitOfWork.BeginTransactionAsync();

        try
        {
            // Get items to return
            var itemsToReturn = originalTransaction.Items.Where(i => itemIds.Contains(i.Id)).ToList();
            if (!itemsToReturn.Any())
            {
                throw new Exception("No valid items found for return");
            }

            // Calculate return totals
            decimal subtotal = -itemsToReturn.Sum(i => i.LineTotal);
            decimal tax = subtotal * TaxRate;
            decimal total = subtotal + tax;

            // Create return transaction (negative amounts)
            var returnTransaction = new Transaction
            {
                TransactionDate = DateTime.UtcNow,
                Subtotal = subtotal,
                Tax = tax,
                Discount = 0,
                Total = total,
                PaymentMethod = originalTransaction.PaymentMethod,
                CashierId = userId,
                IsVoided = false
            };

            await unitOfWork.Transactions.AddAsync(returnTransaction);
            await unitOfWork.SaveChangesAsync();

            // Create return transaction items and restore inventory
            foreach (var item in itemsToReturn)
            {
                var returnItem = new TransactionItem
                {
                    TransactionId = returnTransaction.Id,
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Quantity = -item.Quantity, // Negative quantity for return
                    UnitPrice = item.UnitPrice,
                    LineTotal = -item.LineTotal
                };

                // Restore inventory
                var product = await unitOfWork.Products.GetByIdAsync(item.ProductId);
                if (product != null)
                {
                    product.Quantity += item.Quantity;
                    product.ModifiedAt = DateTime.UtcNow;
                    await unitOfWork.Products.UpdateAsync(product);
                }
            }

            await unitOfWork.SaveChangesAsync();

            // Log audit entry
            var auditLog = new AuditLog
            {
                UserId = userId,
                Action = "PROCESS_RETURN",
                EntityType = "Transaction",
                EntityId = returnTransaction.Id,
                NewValue = $"Return processed for transaction {originalTransactionId}: Total={total:C}",
                Timestamp = DateTime.UtcNow
            };
            await unitOfWork.AuditLogs.AddAsync(auditLog);
            await unitOfWork.SaveChangesAsync();

            await unitOfWork.CommitTransactionAsync();

            return MapToDto(returnTransaction);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<decimal> CalculateTotalAsync(List<TransactionItemDto> items, decimal discount)
    {
        decimal subtotal = items.Sum(i => i.LineTotal);
        decimal tax = subtotal * TaxRate;
        decimal total = subtotal + tax - discount;
        return await Task.FromResult(total);
    }

    public async Task<byte[]> GenerateReceiptAsync(int transactionId)
    {
        var transaction = await unitOfWork.Transactions.GetByIdAsync(transactionId);
        if (transaction == null)
        {
            throw new Exception("Transaction not found");
        }

        // Simple text receipt (in production, use a PDF library like QuestPDF)
        var receipt = new StringBuilder();
        receipt.AppendLine("========================================");
        receipt.AppendLine("           TH-MM POS SYSTEM            ");
        receipt.AppendLine("========================================");
        receipt.AppendLine($"Transaction ID: {transaction.Id}");
        receipt.AppendLine($"Date: {transaction.TransactionDate:yyyy-MM-dd HH:mm:ss}");
        receipt.AppendLine($"Cashier: {transaction.Cashier.Username}");
        receipt.AppendLine("========================================");
        receipt.AppendLine("ITEMS:");
        receipt.AppendLine("----------------------------------------");

        foreach (var item in transaction.Items)
        {
            receipt.AppendLine($"{item.ProductName}");
            receipt.AppendLine($"  {item.Quantity} x {item.UnitPrice:C} = {item.LineTotal:C}");
        }

        receipt.AppendLine("========================================");
        receipt.AppendLine($"Subtotal:        {transaction.Subtotal,15:C}");
        receipt.AppendLine($"Tax (10%):       {transaction.Tax,15:C}");
        receipt.AppendLine($"Discount:        {transaction.Discount,15:C}");
        receipt.AppendLine("----------------------------------------");
        receipt.AppendLine($"TOTAL:           {transaction.Total,15:C}");
        receipt.AppendLine("========================================");
        receipt.AppendLine($"Payment Method: {transaction.PaymentMethod}");
        receipt.AppendLine("========================================");
        receipt.AppendLine("      Thank you for your business!     ");
        receipt.AppendLine("========================================");

        return Encoding.UTF8.GetBytes(receipt.ToString());
    }

    private TransactionDto MapToDto(Transaction transaction)
    {
        return new TransactionDto
        {
            Id = transaction.Id,
            TransactionDate = transaction.TransactionDate,
            Subtotal = transaction.Subtotal,
            Tax = transaction.Tax,
            Discount = transaction.Discount,
            Total = transaction.Total,
            PaymentMethod = transaction.PaymentMethod,
            CashierId = transaction.CashierId,
            CashierName = transaction.Cashier.Username,
            IsVoided = transaction.IsVoided,
            Items = transaction.Items.Select(i => new TransactionItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                LineTotal = i.LineTotal
            }).ToList()
        };
    }
}