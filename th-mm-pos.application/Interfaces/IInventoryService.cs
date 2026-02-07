using th_mm_pos.application.DTOs;

namespace th_mm_pos.application.Interfaces;

public interface IInventoryService
{
    Task<ProductDto> AddProductAsync(ProductDto productDto);
    Task<ProductDto> UpdateProductAsync(int id, ProductDto productDto);
    Task<bool> DeleteProductAsync(int id);
    Task<ProductDto?> GetProductByIdAsync(int id);
    Task<IEnumerable<ProductDto>> SearchProductsAsync(ProductSearchDto searchDto);
    Task<IEnumerable<ProductDto>> GetLowStockProductsAsync();
    Task<bool> AdjustInventoryAsync(int productId, int quantity);
}