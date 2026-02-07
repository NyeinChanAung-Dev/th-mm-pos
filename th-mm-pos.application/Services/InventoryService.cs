using FluentValidation;
using th_mm_pos.application.DTOs;
using th_mm_pos.application.Interfaces;
using th_mm_pos.application.Validators;
using th_mm_pos.domain.Entities;
using th_mm_pos.domain.Interfaces;

namespace th_mm_pos.application.Services;

public class InventoryService(
    IUnitOfWork unitOfWork
) : IInventoryService
{
    private readonly ProductValidator _validator = new();

    public async Task<ProductDto> AddProductAsync(ProductDto productDto)
    {
        // Validate
        var validationResult = await _validator.ValidateAsync(productDto);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        await unitOfWork.BeginTransactionAsync();

        try
        {
            // Check duplicate SKU
            var existingProducts = await unitOfWork.Products.FindAsync(p => p.SKU == productDto.SKU);
            if (existingProducts.Any())
            {
                throw new Exception($"Product with SKU '{productDto.SKU}' already exists");
            }

            // Create product
            var product = new Product
            {
                Name = productDto.Name,
                SKU = productDto.SKU,
                Price = productDto.Price,
                Quantity = productDto.Quantity,
                ReorderLevel = productDto.ReorderLevel,
                Category = productDto.Category,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await unitOfWork.Products.AddAsync(product);
            await unitOfWork.SaveChangesAsync();

            // Log audit entry
            var auditLog = new AuditLog
            {
                UserId = 1, // TODO: Get from current user context
                Action = "CREATE",
                EntityType = "Product",
                EntityId = product.Id,
                NewValue = $"Created product: {product.Name} (SKU: {product.SKU})",
                Timestamp = DateTime.UtcNow
            };
            await unitOfWork.AuditLogs.AddAsync(auditLog);
            await unitOfWork.SaveChangesAsync();

            await unitOfWork.CommitTransactionAsync();

            return MapToDto(product);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<ProductDto> UpdateProductAsync(int id, ProductDto productDto)
    {
        // Validate
        var validationResult = await _validator.ValidateAsync(productDto);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        await unitOfWork.BeginTransactionAsync();

        try
        {
            var product = await unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
            {
                throw new Exception("Product not found");
            }

            // Check duplicate SKU (excluding current product)
            var existingProducts = await unitOfWork.Products.FindAsync(p => p.SKU == productDto.SKU && p.Id != id);
            if (existingProducts.Any())
            {
                throw new Exception($"Product with SKU '{productDto.SKU}' already exists");
            }

            var oldValue = $"{product.Name} (SKU: {product.SKU}, Price: {product.Price}, Qty: {product.Quantity})";

            // Update product
            product.Name = productDto.Name;
            product.SKU = productDto.SKU;
            product.Price = productDto.Price;
            product.Quantity = productDto.Quantity;
            product.ReorderLevel = productDto.ReorderLevel;
            product.Category = productDto.Category;
            product.ModifiedAt = DateTime.UtcNow;

            await unitOfWork.Products.UpdateAsync(product);
            await unitOfWork.SaveChangesAsync();

            // Log audit entry
            var newValue = $"{product.Name} (SKU: {product.SKU}, Price: {product.Price}, Qty: {product.Quantity})";
            var auditLog = new AuditLog
            {
                UserId = 1, // TODO: Get from current user context
                Action = "UPDATE",
                EntityType = "Product",
                EntityId = product.Id,
                OldValue = oldValue,
                NewValue = newValue,
                Timestamp = DateTime.UtcNow
            };
            await unitOfWork.AuditLogs.AddAsync(auditLog);
            await unitOfWork.SaveChangesAsync();

            await unitOfWork.CommitTransactionAsync();

            return MapToDto(product);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        await unitOfWork.BeginTransactionAsync();

        try
        {
            var product = await unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
            {
                return false;
            }

            // Soft delete
            product.IsActive = false;
            product.ModifiedAt = DateTime.UtcNow;

            await unitOfWork.Products.UpdateAsync(product);
            await unitOfWork.SaveChangesAsync();

            // Log audit entry
            var auditLog = new AuditLog
            {
                UserId = 1, // TODO: Get from current user context
                Action = "DELETE",
                EntityType = "Product",
                EntityId = product.Id,
                OldValue = $"Deleted product: {product.Name} (SKU: {product.SKU})",
                Timestamp = DateTime.UtcNow
            };
            await unitOfWork.AuditLogs.AddAsync(auditLog);
            await unitOfWork.SaveChangesAsync();

            await unitOfWork.CommitTransactionAsync();

            return true;
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        var product = await unitOfWork.Products.GetByIdAsync(id);
        return product == null ? null : MapToDto(product);
    }

    public async Task<IEnumerable<ProductDto>> SearchProductsAsync(ProductSearchDto searchDto)
    {
        var products = await unitOfWork.Products.GetAllAsync();

        // Apply filters
        if (!string.IsNullOrEmpty(searchDto.SearchTerm))
        {
            var searchTerm = searchDto.SearchTerm.ToLower();
            products = products.Where(p =>
                p.Name.ToLower().Contains(searchTerm) ||
                p.SKU.ToLower().Contains(searchTerm) ||
                (p.Category != null && p.Category.ToLower().Contains(searchTerm))
            );
        }

        if (!string.IsNullOrEmpty(searchDto.Category))
        {
            products = products.Where(p => p.Category == searchDto.Category);
        }

        if (searchDto.IsActive.HasValue)
        {
            products = products.Where(p => p.IsActive == searchDto.IsActive.Value);
        }

        if (searchDto.LowStockOnly == true)
        {
            products = products.Where(p => p.Quantity < p.ReorderLevel);
        }

        return products.Select(MapToDto);
    }

    public async Task<IEnumerable<ProductDto>> GetLowStockProductsAsync()
    {
        var products = await unitOfWork.Products.FindAsync(p => p.Quantity < p.ReorderLevel && p.IsActive);
        return products.Select(MapToDto);
    }

    public async Task<bool> AdjustInventoryAsync(int productId, int quantity)
    {
        await unitOfWork.BeginTransactionAsync();

        try
        {
            var product = await unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
            {
                return false;
            }

            var newQuantity = product.Quantity + quantity;

            // Prevent negative inventory
            if (newQuantity < 0)
            {
                throw new Exception("Insufficient inventory. Transaction would result in negative stock.");
            }

            var oldQuantity = product.Quantity;
            product.Quantity = newQuantity;
            product.ModifiedAt = DateTime.UtcNow;

            await unitOfWork.Products.UpdateAsync(product);
            await unitOfWork.SaveChangesAsync();

            // Log audit entry
            var auditLog = new AuditLog
            {
                UserId = 1, // TODO: Get from current user context
                Action = "ADJUST_INVENTORY",
                EntityType = "Product",
                EntityId = product.Id,
                OldValue = $"Quantity: {oldQuantity}",
                NewValue = $"Quantity: {newQuantity} (Adjusted by {quantity})",
                Timestamp = DateTime.UtcNow
            };
            await unitOfWork.AuditLogs.AddAsync(auditLog);
            await unitOfWork.SaveChangesAsync();

            await unitOfWork.CommitTransactionAsync();

            return true;
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    private ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            SKU = product.SKU,
            Price = product.Price,
            Quantity = product.Quantity,
            ReorderLevel = product.ReorderLevel,
            Category = product.Category,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt,
            ModifiedAt = product.ModifiedAt
        };
    }
}