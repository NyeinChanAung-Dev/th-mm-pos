using Microsoft.Extensions.DependencyInjection;
using th_mm_pos.application.Interfaces;
using th_mm_pos.application.Services;

namespace th_mm_pos.application;

public static class DependencyInjection
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        // Register Application Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<ISalesService, SalesService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IReportService, ReportService>();

        // Add Memory Cache for reporting
        services.AddMemoryCache();
    }
}