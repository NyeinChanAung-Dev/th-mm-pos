using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using th_mm_pos.application.DTOs;
using th_mm_pos.application.Interfaces;
using th_mm_pos.domain.Enums;
using th_mm_pos.infrastructure.Constants;

namespace th_mm_pos.web.Controllers;

[Authorize]
public class OrderController(
    IOrderService orderService,
    IInventoryService inventoryService,
    ILogger<OrderController> logger
) : Controller
{
    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(claim => claim.Type == Constants.ClaimUserId)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    [HttpGet]
    public async Task<IActionResult> Index(OrderStatus? status = null)
    {
        try
        {
            var searchCriteria = new OrderSearchDto
            {
                Status = status
            };

            var orders = await orderService.SearchOrdersAsync(searchCriteria);
            ViewBag.SelectedStatus = status;
            return View(orders);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error loading orders");
            return View("Error");
        }
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        try
        {
            var searchDto = new ProductSearchDto { SearchTerm = "" };
            var products = await inventoryService.SearchProductsAsync(searchDto);
            ViewBag.Products = products.Where(p => p.IsActive);
            return View();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error loading create order form");
            return View("Error");
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create(OrderDto model)
    {
        if (!ModelState.IsValid)
        {
            var searchDto = new ProductSearchDto { SearchTerm = "" };
            var products = await inventoryService.SearchProductsAsync(searchDto);
            ViewBag.Products = products.Where(p => p.IsActive);
            return View(model);
        }

        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized();
            }

            await orderService.CreateOrderAsync(model, userId);
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating order");
            ModelState.AddModelError("", ex.Message);
            var searchDto = new ProductSearchDto { SearchTerm = "" };
            var products = await inventoryService.SearchProductsAsync(searchDto);
            ViewBag.Products = products.Where(p => p.IsActive);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var order = await orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error loading order details");
            return View("Error");
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStatus(int id, OrderStatus newStatus)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized();
            }

            await orderService.UpdateOrderStatusAsync(id, newStatus, userId);
            return RedirectToAction("Details", new { id });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating order status");
            return RedirectToAction("Details", new { id });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Cancel(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized();
            }

            await orderService.CancelOrderAsync(id, userId);
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error cancelling order");
            return RedirectToAction("Details", new { id });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Complete(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized();
            }

            await orderService.CompleteOrderAsync(id, userId);
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error completing order");
            return RedirectToAction("Details", new { id });
        }
    }
}