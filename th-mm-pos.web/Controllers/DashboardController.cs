using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using th_mm_pos.application.Interfaces;

namespace th_mm_pos.web.Controllers;

[Authorize]
public class DashboardController(
    IReportService reportService,
    ILogger<DashboardController> logger
) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var metrics = await reportService.GetDashboardMetricsAsync();
            return View(metrics);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error loading dashboard");
            return View("Error");
        }
    }

    [HttpGet]
    public async Task<IActionResult> SalesReport(DateTime? startDate, DateTime? endDate)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            var report = await reportService.GenerateSalesReportAsync(start, end);

            ViewBag.StartDate = start;
            ViewBag.EndDate = end;

            return View(report);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating sales report");
            return View("Error");
        }
    }

    [HttpGet]
    public async Task<IActionResult> ExportReport(DateTime? startDate, DateTime? endDate)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            var csvData = await reportService.ExportReportToCsvAsync(start, end);
            return File(csvData, "text/csv", $"SalesReport_{start:yyyyMMdd}_{end:yyyyMMdd}.csv");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error exporting report");
            return RedirectToAction("Index");
        }
    }
}