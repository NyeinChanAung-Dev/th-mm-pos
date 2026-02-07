using System.Text;
using Microsoft.Extensions.Caching.Memory;
using th_mm_pos.application.DTOs;
using th_mm_pos.application.Interfaces;
using th_mm_pos.domain.Interfaces;

namespace th_mm_pos.application.Services;

public class ReportService(
    IUnitOfWork unitOfWork,
    IMemoryCache cache
) : IReportService
{
    private const string DashboardMetricsCacheKey = "DashboardMetrics";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public async Task<DashboardMetrics> GetDashboardMetricsAsync()
    {
        // Check cache first
        if (cache.TryGetValue(DashboardMetricsCacheKey, out DashboardMetrics? cachedMetrics) && cachedMetrics != null)
        {
            return cachedMetrics;
        }

        var today = DateTime.UtcNow.Date;
        var transactions = await unitOfWork.Transactions
            .FindAsync(t => t.TransactionDate >= today && !t.IsVoided);

        var allTransactions = await unitOfWork.Transactions
            .FindAsync(t => !t.IsVoided);

        var lowStockProducts = await unitOfWork.Products
            .FindAsync(p => p.IsActive && p.Quantity < p.ReorderLevel);

        var topProducts = await GetTopSellingProductsAsync(5);

        var metrics = new DashboardMetrics
        {
            DailySales = transactions.Sum(t => t.Total),
            TotalRevenue = allTransactions.Sum(t => t.Total),
            TotalTransactionsToday = transactions.Count(),
            LowStockProductCount = lowStockProducts.Count(),
            TopSellingProducts = topProducts.ToList()
        };

        // Cache the metrics
        cache.Set(DashboardMetricsCacheKey, metrics, CacheDuration);

        return metrics;
    }

    public async Task<SalesReport> GenerateSalesReportAsync(DateTime startDate, DateTime endDate)
    {
        var transactions = await unitOfWork.Transactions
            .FindAsync(t => t.TransactionDate >= startDate &&
                            t.TransactionDate <= endDate &&
                            !t.IsVoided);

        var transactionsList = transactions.ToList();

        var totalRevenue = transactionsList.Sum(t => t.Total);
        var totalTransactions = transactionsList.Count;

        var revenueByPaymentMethod = transactionsList
            .GroupBy(t => t.PaymentMethod)
            .ToDictionary(
                g => g.Key.ToString(),
                g => g.Sum(t => t.Total)
            );

        var topProducts = await GetTopSellingProductsInPeriodAsync(startDate, endDate, 10);

        return new SalesReport
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalRevenue = totalRevenue,
            TotalTax = transactionsList.Sum(t => t.Tax),
            TotalDiscount = transactionsList.Sum(t => t.Discount),
            TotalTransactions = totalTransactions,
            AverageTransactionValue = totalTransactions > 0 ? totalRevenue / totalTransactions : 0,
            RevenueByPaymentMethod = revenueByPaymentMethod,
            TopSellingProducts = topProducts.ToList()
        };
    }

    public async Task<IEnumerable<ProductSalesDto>> GetTopSellingProductsAsync(int count)
    {
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        return await GetTopSellingProductsInPeriodAsync(thirtyDaysAgo, DateTime.UtcNow, count);
    }

    private async Task<IEnumerable<ProductSalesDto>> GetTopSellingProductsInPeriodAsync(
        DateTime startDate, DateTime endDate, int count)
    {
        var transactions = await unitOfWork.Transactions
            .FindAsync(t => t.TransactionDate >= startDate &&
                            t.TransactionDate <= endDate &&
                            !t.IsVoided);

        var transactionIds = transactions.Select(t => t.Id).ToList();

        var transactionItems = await unitOfWork.TransactionItems
            .FindAsync(ti => transactionIds.Contains(ti.TransactionId));

        var productSales = transactionItems
            .GroupBy(ti => new { ti.ProductId, ti.ProductName })
            .Select(g => new ProductSalesDto
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.ProductName,
                SKU = string.Empty, // Will be populated from product
                TotalQuantitySold = g.Sum(ti => ti.Quantity),
                TotalRevenue = g.Sum(ti => ti.LineTotal),
                TransactionCount = g.Select(ti => ti.TransactionId).Distinct().Count()
            })
            .OrderByDescending(ps => ps.TotalRevenue)
            .Take(count)
            .ToList();

        // Populate SKU from products
        foreach (var productSale in productSales)
        {
            var product = await unitOfWork.Products.GetByIdAsync(productSale.ProductId);
            if (product != null)
            {
                productSale.SKU = product.SKU;
            }
        }

        return productSales;
    }

    public async Task<byte[]> ExportReportToCsvAsync(DateTime startDate, DateTime endDate)
    {
        var report = await GenerateSalesReportAsync(startDate, endDate);

        var csv = new StringBuilder();
        csv.AppendLine("Sales Report");
        csv.AppendLine($"Period: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
        csv.AppendLine();
        csv.AppendLine("Summary");
        csv.AppendLine($"Total Revenue,{report.TotalRevenue:C}");
        csv.AppendLine($"Total Tax,{report.TotalTax:C}");
        csv.AppendLine($"Total Discount,{report.TotalDiscount:C}");
        csv.AppendLine($"Total Transactions,{report.TotalTransactions}");
        csv.AppendLine($"Average Transaction Value,{report.AverageTransactionValue:C}");
        csv.AppendLine();
        csv.AppendLine("Revenue by Payment Method");
        csv.AppendLine("Payment Method,Revenue");
        foreach (var kvp in report.RevenueByPaymentMethod)
        {
            csv.AppendLine($"{kvp.Key},{kvp.Value:C}");
        }

        csv.AppendLine();
        csv.AppendLine("Top Selling Products");
        csv.AppendLine("Product Name,SKU,Quantity Sold,Revenue,Transactions");
        foreach (var product in report.TopSellingProducts)
        {
            csv.AppendLine(
                $"{product.ProductName},{product.SKU},{product.TotalQuantitySold},{product.TotalRevenue:C},{product.TransactionCount}");
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }
}