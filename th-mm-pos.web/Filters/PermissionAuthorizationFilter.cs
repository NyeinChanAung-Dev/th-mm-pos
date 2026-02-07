using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using th_mm_pos.application.Interfaces;
using th_mm_pos.domain.Enums;

namespace th_mm_pos.web.Filters;

public class PermissionAuthorizationFilter(
    PermissionType requiredPermission,
    IPermissionService permissionService,
    ILogger<PermissionAuthorizationFilter> logger
) : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        // Check if user is authenticated
        var userId = context.HttpContext.Session.GetInt32("UserId");
        if (!userId.HasValue)
        {
            context.Result = new RedirectToActionResult("Login", "Auth", null);
            return;
        }

        // Check if user has required permission
        var hasPermission = await permissionService.CheckPermissionAsync(userId.Value, requiredPermission.ToString());

        if (!hasPermission)
        {
            logger.LogWarning(
                "User {UserId} attempted to access {Action} without {Permission} permission",
                userId.Value,
                context.ActionDescriptor.DisplayName,
                requiredPermission);

            // Log permission denial for audit
            // This would be handled by the PermissionService in a real implementation

            context.Result = new ViewResult
            {
                ViewName = "AccessDenied",
                StatusCode = 403
            };
        }
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class RequirePermissionAttribute : TypeFilterAttribute
{
    public RequirePermissionAttribute(PermissionType permission)
        : base(typeof(PermissionAuthorizationFilter))
    {
        Arguments = new object[] { permission };
    }
}