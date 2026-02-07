using th_mm_pos.application.DTOs;

namespace th_mm_pos.application.Interfaces;

public interface IReportService
{
    Task<DashboardMetrics> GetDashboardMetricsAsync();
    Task<SalesReport> GenerateSalesReportAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<ProductSalesDto>> GetTopSellingProductsAsync(int count);
    Task<byte[]> ExportReportToCsvAsync(DateTime startDate, DateTime endDate);
}