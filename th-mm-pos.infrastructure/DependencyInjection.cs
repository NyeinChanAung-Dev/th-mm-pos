using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using th_mm_pos.domain.Interfaces;
using th_mm_pos.infrastructure.Data;
using th_mm_pos.infrastructure.Repositories;

namespace th_mm_pos.infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Register Repositories and UnitOfWork
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }
}