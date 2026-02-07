using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using th_mm_pos.application.DTOs;
using th_mm_pos.application.Interfaces;

namespace th_mm_pos.web.Controllers;

[Authorize]
public class AdminController(
    IPermissionService permissionService,
    ILogger<AdminController> logger
) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var users = await permissionService.GetUsersAsync();
            return View(users);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error loading users");
            return View("Error");
        }
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(UserCreateDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await permissionService.CreateUserAsync(model);
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating user");
            ModelState.AddModelError("", "An error occurred while creating the user");
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var model = new UserUpdateDto { UserId = id };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, UserUpdateDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            model.UserId = id;
            await permissionService.UpdateUserPermissionsAsync(model);
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating user");
            ModelState.AddModelError("", "An error occurred while updating the user");
            return View(model);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Deactivate(int id)
    {
        try
        {
            await permissionService.DeactivateUserAsync(id);
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deactivating user");
            return RedirectToAction("Index");
        }
    }
}