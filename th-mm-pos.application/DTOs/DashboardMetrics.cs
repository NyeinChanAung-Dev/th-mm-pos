namespace th_mm_pos.application.DTOs;

public class DashboardMetrics
{
    public decimal DailySales { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalTransactionsToday { get; set; }
    public int LowStockProductCount { get; set; }
    public List<ProductSalesDto> TopSellingProducts { get; set; } = new();
}