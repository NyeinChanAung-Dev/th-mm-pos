using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using th_mm_pos.application.DTOs;
using th_mm_pos.application.Interfaces;
using th_mm_pos.infrastructure.Constants;

namespace th_mm_pos.web.Controllers;

public class AuthController(
    IAuthService authService,
    ILogger<AuthController> logger
) : Controller
{
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var result = await authService.LoginAsync(model.Username, model.Password);

            if (result is { Success: true, User: not null })
            {
                // Create claims for cookie authentication
                var claims = new List<Claim>
                {
                    new(Constants.ClaimUserId, result.User.Id.ToString()),
                    new(Constants.ClaimUserName, result.User.Username)
                };
                claims.AddRange(result.User.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

                // Add roles as claims

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                return RedirectToAction("Index", "Dashboard");
            }

            ModelState.AddModelError("", "Invalid username or password");
            return View(model);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during login");
            ModelState.AddModelError("", "An error occurred during login");
            return View(model);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        try
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during logout");
            return RedirectToAction("Login");
        }
    }
}