using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using th_mm_pos.application.DTOs;
using th_mm_pos.application.Interfaces;
using th_mm_pos.infrastructure.Constants;

namespace th_mm_pos.web.Controllers;

[Authorize]
public class SalesController(
    ISalesService salesService,
    IInventoryService inventoryService,
    ILogger<SalesController> logger
) : Controller
{
    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(claim => claim.Type == Constants.ClaimUserId)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var searchDto = new ProductSearchDto { SearchTerm = "" };
            var products = await inventoryService.SearchProductsAsync(searchDto);
            var activeProducts = products.Where(p => p.IsActive && p.Quantity > 0);
            return View(activeProducts);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error loading POS interface");
            return View("Error");
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TransactionDto model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized();
            }

            var transaction = await salesService.CreateTransactionAsync(model, userId);

            return Ok(new { success = true, transactionId = transaction.Id });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating transaction");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Void(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized();
            }

            await salesService.VoidTransactionAsync(id, userId);
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error voiding transaction");
            return RedirectToAction("Index");
        }
    }

    [HttpGet]
    public IActionResult Return()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Return(int transactionId, List<int> itemIds)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized();
            }

            await salesService.ProcessReturnAsync(transactionId, itemIds, userId);
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing return");
            ModelState.AddModelError("", ex.Message);
            return View();
        }
    }

    [HttpGet]
    public async Task<IActionResult> Receipt(int id)
    {
        try
        {
            var receiptData = await salesService.GenerateReceiptAsync(id);
            return File(receiptData, "application/pdf", $"Receipt_{id}.pdf");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating receipt");
            return RedirectToAction("Index");
        }
    }
}