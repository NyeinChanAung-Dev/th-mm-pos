using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using th_mm_pos.application.DTOs;
using th_mm_pos.application.Interfaces;

namespace th_mm_pos.web.Controllers;

[Authorize]
public class InventoryController(
    IInventoryService inventoryService,
    ILogger<InventoryController> logger
) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string searchTerm = "")
    {
        try
        {
            IEnumerable<ProductDto> products;

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchDto = new ProductSearchDto { SearchTerm = "" };
                products = await inventoryService.SearchProductsAsync(searchDto);
            }
            else
            {
                var searchDto = new ProductSearchDto { SearchTerm = searchTerm };
                products = await inventoryService.SearchProductsAsync(searchDto);
            }

            ViewBag.SearchTerm = searchTerm;
            return View(products);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error loading products");
            return View("Error");
        }
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(ProductDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await inventoryService.AddProductAsync(model);
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating product");
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var product = await inventoryService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error loading product");
            return View("Error");
        }
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, ProductDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await inventoryService.UpdateProductAsync(id, model);
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating product");
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await inventoryService.DeleteProductAsync(id);
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting product");
            return RedirectToAction("Index");
        }
    }

    [HttpGet]
    public async Task<IActionResult> LowStock()
    {
        try
        {
            var products = await inventoryService.GetLowStockProductsAsync();
            return View(products);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error loading low stock products");
            return View("Error");
        }
    }
}