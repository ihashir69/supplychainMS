// ============================================================
// StoresController.cs — Handles all /Stores/* URLs
// ============================================================
//
// This controller has two audiences:
//
//   STORE MANAGER:
//     - View and edit their OWN store profile
//     - Full inventory management (add products, update stock, remove)
//
//   SUPPLIER (read-only):
//     - Browse the list of all stores (their customers)
//     - View a store's profile (not inventory — that's private)
//
// The Inventory actions are StoreManager-only.
// Store profile viewing is open to both roles.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SupplyChainMS.Models;
using SupplyChainMS.Services.Interfaces;
using SupplyChainMS.ViewModels;

namespace SupplyChainMS.Controllers;

[Authorize(Roles = "StoreManager,Supplier")]
public class StoresController : Controller
{
    private readonly IStoreService _storeService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<StoresController> _logger;

    public StoresController(
        IStoreService storeService,
        UserManager<ApplicationUser> userManager,
        ILogger<StoresController> logger)
    {
        _storeService = storeService;
        _userManager = userManager;
        _logger = logger;
    }

    // -------------------------------------------------------
    // Helper: gets the logged-in StoreManager's store.
    // Returns null if not found.
    // -------------------------------------------------------
    private async Task<Store?> GetCurrentStoreAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return null;
        return await _storeService.GetByUserIdAsync(user.Id);
    }

    // -------------------------------------------------------
    // GET /Stores
    // StoreManager → redirected to their own store details
    // Supplier → sees a list of all stores
    // -------------------------------------------------------
    public async Task<IActionResult> Index()
    {
        ViewData["ActivePage"] = "Stores";

        if (User.IsInRole("StoreManager"))
        {
            var store = await GetCurrentStoreAsync();
            if (store == null)
            {
                TempData["Error"] = "Store profile not found. Please contact support.";
                return RedirectToAction("Index", "Home");
            }
            return RedirectToAction(nameof(Details), new { id = store.Id });
        }

        var stores = await _storeService.GetAllActiveAsync();
        return View(stores);
    }

    // -------------------------------------------------------
    // GET /Stores/Details/3
    // View a store's profile. Both roles can access.
    // -------------------------------------------------------
    public async Task<IActionResult> Details(int id)
    {
        ViewData["ActivePage"] = "Stores";

        var store = await _storeService.GetByIdAsync(id);
        if (store == null)
        {
            TempData["Error"] = "Store not found.";
            return RedirectToAction(nameof(Index));
        }

        return View(store);
    }

    // -------------------------------------------------------
    // GET /Stores/EditProfile
    // StoreManager edits their own store's profile.
    // -------------------------------------------------------
    [Authorize(Roles = "StoreManager")]
    public async Task<IActionResult> EditProfile()
    {
        ViewData["ActivePage"] = "Stores";

        var store = await GetCurrentStoreAsync();
        if (store == null)
        {
            TempData["Error"] = "Store profile not found.";
            return RedirectToAction("Index", "Home");
        }

        var viewModel = new StoreProfileEditViewModel
        {
            Id = store.Id,
            Name = store.Name,
            Address = store.Address,
            ContactPhone = store.ContactPhone,
            ContactEmail = store.ContactEmail
        };

        return View(viewModel);
    }

    // -------------------------------------------------------
    // POST /Stores/EditProfile
    // -------------------------------------------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "StoreManager")]
    public async Task<IActionResult> EditProfile(StoreProfileEditViewModel viewModel)
    {
        ViewData["ActivePage"] = "Stores";

        if (!ModelState.IsValid)
            return View(viewModel);

        var user = await _userManager.GetUserAsync(User);
        var existingStore = await _storeService.GetByUserIdAsync(user!.Id);

        if (existingStore == null || existingStore.Id != viewModel.Id)
        {
            TempData["Error"] = "Unauthorized action.";
            return RedirectToAction("Index", "Home");
        }

        existingStore.Name = viewModel.Name;
        existingStore.Address = viewModel.Address;
        existingStore.ContactPhone = viewModel.ContactPhone;
        existingStore.ContactEmail = viewModel.ContactEmail;

        try
        {
            await _storeService.UpdateProfileAsync(existingStore);
            TempData["Success"] = "Store profile updated successfully!";
            return RedirectToAction(nameof(Details), new { id = existingStore.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating store profile for user {UserId}", user.Id);
            TempData["Error"] = "An error occurred while saving. Please try again.";
            return View(viewModel);
        }
    }

    // -------------------------------------------------------
    // GET /Stores/Inventory
    // Shows the store's full inventory with low stock alerts.
    // Linked directly from the sidebar.
    // -------------------------------------------------------
    [Authorize(Roles = "StoreManager")]
    public async Task<IActionResult> Inventory()
    {
        ViewData["ActivePage"] = "Inventory";

        var store = await GetCurrentStoreAsync();
        if (store == null)
        {
            TempData["Error"] = "Store profile not found.";
            return RedirectToAction("Index", "Home");
        }

        var inventory = await _storeService.GetInventoryAsync(store.Id);

        // Pass store info to the view for the page heading
        ViewBag.Store = store;

        // Count how many items are at or below threshold (for the alert badge)
        ViewBag.LowStockCount = inventory.Count(i => i.QuantityInStock <= i.LowStockThreshold);

        return View(inventory);
    }

    // -------------------------------------------------------
    // GET /Stores/AddProduct
    // Form to add a new product to the store's inventory.
    // -------------------------------------------------------
    [Authorize(Roles = "StoreManager")]
    public async Task<IActionResult> AddProduct()
    {
        ViewData["ActivePage"] = "Inventory";

        var store = await GetCurrentStoreAsync();
        if (store == null)
        {
            TempData["Error"] = "Store profile not found.";
            return RedirectToAction("Index", "Home");
        }

        // Load only the products not already in inventory
        var availableProducts = await _storeService.GetAvailableProductsToAddAsync(store.Id);

        if (!availableProducts.Any())
        {
            TempData["Success"] = "All available products are already in your inventory!";
            return RedirectToAction(nameof(Inventory));
        }

        // Build a SelectList for the dropdown.
        // Format: "SupplierName — ProductName (PKR price/unit)"
        // SelectList(items, valueField, textField) — "valueField" is what gets submitted,
        // "textField" is what the user sees in the dropdown.
        var selectItems = availableProducts.Select(p => new SelectListItem
        {
            Value = p.Id.ToString(),
            Text = $"{p.Supplier.CompanyName} — {p.Name} (PKR {p.UnitPrice:N0}/{p.Unit})"
        });

        ViewBag.ProductList = new SelectList(selectItems, "Value", "Text");
        ViewBag.StoreId = store.Id;

        var viewModel = new AddInventoryItemViewModel
        {
            StoreId = store.Id,
            LowStockThreshold = 10
        };

        return View(viewModel);
    }

    // -------------------------------------------------------
    // POST /Stores/AddProduct
    // -------------------------------------------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "StoreManager")]
    public async Task<IActionResult> AddProduct(AddInventoryItemViewModel viewModel)
    {
        ViewData["ActivePage"] = "Inventory";

        var store = await GetCurrentStoreAsync();
        if (store == null || store.Id != viewModel.StoreId)
        {
            TempData["Error"] = "Unauthorized action.";
            return RedirectToAction("Index", "Home");
        }

        if (!ModelState.IsValid)
        {
            // Re-populate the dropdown before returning to the form
            var availableProducts = await _storeService.GetAvailableProductsToAddAsync(store.Id);
            var selectItems = availableProducts.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = $"{p.Supplier.CompanyName} — {p.Name} (PKR {p.UnitPrice:N0}/{p.Unit})"
            });
            ViewBag.ProductList = new SelectList(selectItems, "Value", "Text");
            ViewBag.StoreId = store.Id;
            return View(viewModel);
        }

        // Check for duplicate
        var alreadyExists = await _storeService.IsProductInInventoryAsync(store.Id, viewModel.ProductId);
        if (alreadyExists)
        {
            TempData["Error"] = "This product is already in your inventory. Use 'Update Stock' to change the quantity.";
            return RedirectToAction(nameof(Inventory));
        }

        var item = new InventoryItem
        {
            StoreId = store.Id,
            ProductId = viewModel.ProductId,
            QuantityInStock = viewModel.QuantityInStock,
            LowStockThreshold = viewModel.LowStockThreshold
        };

        try
        {
            await _storeService.AddInventoryItemAsync(item);
            TempData["Success"] = "Product added to inventory!";
            return RedirectToAction(nameof(Inventory));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding product {ProductId} to store {StoreId}", viewModel.ProductId, store.Id);
            TempData["Error"] = "An error occurred. Please try again.";
            return RedirectToAction(nameof(Inventory));
        }
    }

    // -------------------------------------------------------
    // GET /Stores/UpdateStock/7
    // Form to update stock quantity + low stock threshold.
    // -------------------------------------------------------
    [Authorize(Roles = "StoreManager")]
    public async Task<IActionResult> UpdateStock(int id)
    {
        ViewData["ActivePage"] = "Inventory";

        var store = await GetCurrentStoreAsync();
        if (store == null)
        {
            TempData["Error"] = "Store profile not found.";
            return RedirectToAction("Index", "Home");
        }

        var item = await _storeService.GetInventoryItemAsync(id);
        if (item == null || item.StoreId != store.Id)
        {
            TempData["Error"] = "Inventory item not found or access denied.";
            return RedirectToAction(nameof(Inventory));
        }

        var viewModel = new UpdateStockViewModel
        {
            InventoryItemId = item.Id,
            ProductName = item.Product.Name,
            SupplierName = item.Product.Supplier.CompanyName,
            QuantityInStock = item.QuantityInStock,
            LowStockThreshold = item.LowStockThreshold
        };

        return View(viewModel);
    }

    // -------------------------------------------------------
    // POST /Stores/UpdateStock/7
    // -------------------------------------------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "StoreManager")]
    public async Task<IActionResult> UpdateStock(UpdateStockViewModel viewModel)
    {
        ViewData["ActivePage"] = "Inventory";

        if (!ModelState.IsValid)
            return View(viewModel);

        var store = await GetCurrentStoreAsync();
        if (store == null)
        {
            TempData["Error"] = "Store profile not found.";
            return RedirectToAction("Index", "Home");
        }

        var item = await _storeService.GetInventoryItemAsync(viewModel.InventoryItemId);
        if (item == null || item.StoreId != store.Id)
        {
            TempData["Error"] = "Unauthorized action.";
            return RedirectToAction(nameof(Inventory));
        }

        // Update only the fields the StoreManager can change.
        // We re-use the fetched item but need a tracked version for update.
        var updatedItem = new InventoryItem
        {
            Id = item.Id,
            StoreId = item.StoreId,
            ProductId = item.ProductId,
            QuantityInStock = viewModel.QuantityInStock,
            LowStockThreshold = viewModel.LowStockThreshold
        };

        try
        {
            await _storeService.UpdateInventoryItemAsync(updatedItem);
            TempData["Success"] = $"Stock updated for {viewModel.ProductName}.";
            return RedirectToAction(nameof(Inventory));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating stock for inventory item {ItemId}", viewModel.InventoryItemId);
            TempData["Error"] = "An error occurred. Please try again.";
            return View(viewModel);
        }
    }

    // -------------------------------------------------------
    // POST /Stores/RemoveProduct/7
    // Removes a product from inventory (no confirmation page —
    // the Inventory view has an inline confirmation via JavaScript).
    // -------------------------------------------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "StoreManager")]
    public async Task<IActionResult> RemoveProduct(int id)
    {
        var store = await GetCurrentStoreAsync();
        if (store == null)
        {
            TempData["Error"] = "Store profile not found.";
            return RedirectToAction("Index", "Home");
        }

        var item = await _storeService.GetInventoryItemAsync(id);
        if (item == null || item.StoreId != store.Id)
        {
            TempData["Error"] = "Unauthorized action.";
            return RedirectToAction(nameof(Inventory));
        }

        try
        {
            await _storeService.RemoveInventoryItemAsync(id);
            TempData["Success"] = $"{item.Product.Name} removed from inventory.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing inventory item {ItemId}", id);
            TempData["Error"] = "Could not remove product. It may be linked to existing orders.";
        }

        return RedirectToAction(nameof(Inventory));
    }
}
