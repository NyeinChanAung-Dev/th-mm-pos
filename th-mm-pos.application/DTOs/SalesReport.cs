namespace th_mm_pos.application.DTOs;

public class SalesReport
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalTax { get; set; }
    public decimal TotalDiscount { get; set; }
    public int TotalTransactions { get; set; }
    public decimal AverageTransactionValue { get; set; }
    public Dictionary<string, decimal> RevenueByPaymentMethod { get; set; } = new();
    public List<ProductSalesDto> TopSellingProducts { get; set; } = new();
}