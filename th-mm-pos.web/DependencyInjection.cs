using Microsoft.AspNetCore.Authentication.Cookies;
using th_mm_pos.web.Middleware;

namespace th_mm_pos.web;

public static class DependencyInjection
{
    public static void AddWebServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        var sessionExpireTimeInMinutes = Convert.ToInt32(configuration.GetSection("AppSettings:SessionExpireTimeInMinutes").Value);
        
        // Configure Cookie Authentication
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(config =>
            {
                config.LoginPath = "/Auth/Login";
                config.AccessDeniedPath = "/Shared/AccessDenied";
                config.ExpireTimeSpan = TimeSpan.FromMinutes(sessionExpireTimeInMinutes);
                config.SlidingExpiration = true;
                config.Cookie.HttpOnly = true;
                config.Cookie.IsEssential = true;
            });

        // Configure Authorization
        services.AddAuthorization();

        // Configure Session
        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(sessionExpireTimeInMinutes);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });

        // Configure CORS
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        // Register Global Exception Handler
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
    }
}