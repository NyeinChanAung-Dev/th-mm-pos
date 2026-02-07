using th_mm_pos.application.DTOs;
using th_mm_pos.application.Interfaces;
using th_mm_pos.domain.Entities;
using th_mm_pos.domain.Enums;
using th_mm_pos.domain.Interfaces;

namespace th_mm_pos.application.Services;

public class OrderService(
    IUnitOfWork unitOfWork,
    ISalesService salesService
) : IOrderService
{
    public async Task<OrderDto> CreateOrderAsync(OrderDto orderDto, int userId)
    {
        if (orderDto.Items.Count == 0)
        {
            throw new Exception("Order must have at least one item");
        }

        await unitOfWork.BeginTransactionAsync();

        try
        {
            // Validate products exist
            foreach (var item in orderDto.Items)
            {
                var product = await unitOfWork.Products.GetByIdAsync(item.ProductId);
                if (product == null)
                {
                    throw new Exception($"Product with ID {item.ProductId} not found");
                }
            }

            // Create order
            var order = new Order
            {
                CustomerName = orderDto.CustomerName,
                CustomerPhone = orderDto.CustomerPhone,
                OrderDate = DateTime.UtcNow,
                ExpectedFulfillmentDate = orderDto.ExpectedFulfillmentDate,
                Status = OrderStatus.Pending,
                CreatedByUserId = userId
            };

            await unitOfWork.Orders.AddAsync(order);
            await unitOfWork.SaveChangesAsync();

            // Create order items
            foreach (var itemDto in orderDto.Items)
            {
                await unitOfWork.Products.GetByIdAsync(itemDto.ProductId);
            }

            await unitOfWork.SaveChangesAsync();

            // Log audit entry
            var auditLog = new AuditLog
            {
                UserId = userId,
                Action = "CREATE_ORDER",
                EntityType = "Order",
                EntityId = order.Id,
                NewValue = $"Order created for {order.CustomerName}: {orderDto.Items.Count} items",
                Timestamp = DateTime.UtcNow
            };
            await unitOfWork.AuditLogs.AddAsync(auditLog);
            await unitOfWork.SaveChangesAsync();

            await unitOfWork.CommitTransactionAsync();

            return MapToDto(order);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<OrderDto> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus, int userId)
    {
        var order = await unitOfWork.Orders.GetByIdAsync(orderId);
        if (order == null)
        {
            throw new Exception("Order not found");
        }

        var oldStatus = order.Status;
        order.Status = newStatus;

        await unitOfWork.Orders.UpdateAsync(order);
        await unitOfWork.SaveChangesAsync();

        // Log audit entry
        var auditLog = new AuditLog
        {
            UserId = userId,
            Action = "UPDATE_ORDER_STATUS",
            EntityType = "Order",
            EntityId = order.Id,
            OldValue = $"Status: {oldStatus}",
            NewValue = $"Status: {newStatus}",
            Timestamp = DateTime.UtcNow
        };
        await unitOfWork.AuditLogs.AddAsync(auditLog);
        await unitOfWork.SaveChangesAsync();

        return MapToDto(order);
    }

    public async Task<OrderDto?> GetOrderByIdAsync(int orderId)
    {
        var order = await unitOfWork.Orders.GetByIdAsync(orderId);
        return order == null ? null : MapToDto(order);
    }

    public async Task<IEnumerable<OrderDto>> SearchOrdersAsync(OrderSearchDto searchDto)
    {
        var orders = await unitOfWork.Orders.GetAllAsync();

        // Apply filters
        if (searchDto.Status.HasValue)
        {
            orders = orders.Where(o => o.Status == searchDto.Status.Value);
        }

        if (searchDto.StartDate.HasValue)
        {
            orders = orders.Where(o => o.OrderDate >= searchDto.StartDate.Value);
        }

        if (searchDto.EndDate.HasValue)
        {
            orders = orders.Where(o => o.OrderDate <= searchDto.EndDate.Value);
        }

        if (!string.IsNullOrEmpty(searchDto.CustomerName))
        {
            var searchTerm = searchDto.CustomerName.ToLower();
            orders = orders.Where(o => o.CustomerName.ToLower().Contains(searchTerm));
        }

        return orders.Select(MapToDto);
    }

    public async Task<bool> CancelOrderAsync(int orderId, int userId)
    {
        var order = await unitOfWork.Orders.GetByIdAsync(orderId);
        if (order == null)
        {
            return false;
        }

        if (order.Status == OrderStatus.Completed || order.Status == OrderStatus.Cancelled)
        {
            throw new Exception("Cannot cancel a completed or already cancelled order");
        }

        order.Status = OrderStatus.Cancelled;
        await unitOfWork.Orders.UpdateAsync(order);
        await unitOfWork.SaveChangesAsync();

        // Log audit entry
        var auditLog = new AuditLog
        {
            UserId = userId,
            Action = "CANCEL_ORDER",
            EntityType = "Order",
            EntityId = order.Id,
            OldValue = $"Order cancelled: {order.CustomerName}",
            Timestamp = DateTime.UtcNow
        };
        await unitOfWork.AuditLogs.AddAsync(auditLog);
        await unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<TransactionDto> CompleteOrderAsync(int orderId, int userId)
    {
        var order = await unitOfWork.Orders.GetByIdAsync(orderId);
        if (order == null)
        {
            throw new Exception("Order not found");
        }

        if (order.Status == OrderStatus.Completed)
        {
            throw new Exception("Order is already completed");
        }

        if (order.Status == OrderStatus.Cancelled)
        {
            throw new Exception("Cannot complete a cancelled order");
        }

        // Create transaction from order
        var transactionDto = new TransactionDto
        {
            Discount = 0,
            PaymentMethod = PaymentMethod.Cash, // Default, should be provided by caller
            Items = order.Items.Select(i => new TransactionItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                LineTotal = i.Quantity * i.UnitPrice
            }).ToList()
        };

        var transaction = await salesService.CreateTransactionAsync(transactionDto, userId);

        // Update order status
        order.Status = OrderStatus.Completed;
        order.CompletedTransactionId = transaction.Id;
        await unitOfWork.Orders.UpdateAsync(order);
        await unitOfWork.SaveChangesAsync();

        // Log audit entry
        var auditLog = new AuditLog
        {
            UserId = userId,
            Action = "COMPLETE_ORDER",
            EntityType = "Order",
            EntityId = order.Id,
            NewValue = $"Order completed with transaction {transaction.Id}",
            Timestamp = DateTime.UtcNow
        };
        await unitOfWork.AuditLogs.AddAsync(auditLog);
        await unitOfWork.SaveChangesAsync();

        return transaction;
    }

    private static OrderDto MapToDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            CustomerName = order.CustomerName,
            CustomerPhone = order.CustomerPhone,
            OrderDate = order.OrderDate,
            ExpectedFulfillmentDate = order.ExpectedFulfillmentDate,
            Status = order.Status,
            CreatedByUserId = order.CreatedByUserId,
            CreatedByUsername = order.CreatedBy.Username,
            CompletedTransactionId = order.CompletedTransactionId,
            Items = order.Items.Select(i => new OrderItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                IsFulfilled = i.IsFulfilled
            }).ToList()
        };
    }
}