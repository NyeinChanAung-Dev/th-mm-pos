namespace th_mm_pos.application.DTOs;

public class ProductSearchDto
{
    public string? SearchTerm { get; set; }
    public string? Category { get; set; }
    public bool? IsActive { get; set; }
    public bool? LowStockOnly { get; set; }
}