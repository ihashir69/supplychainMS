// ============================================================
// StoresController.cs — HQ manages ALL store branches
// ============================================================
//
// StoreManager = HQ. Sees and manages every branch.
// No "my store" concept — HQ picks which branch to work with.
//
// Supplier can browse the branch directory (read-only).

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

    public StoresController(IStoreService storeService, UserManager<ApplicationUser> userManager, ILogger<StoresController> logger)
    {
        _storeService = storeService;
        _userManager = userManager;
        _logger = logger;
    }

    // -------------------------------------------------------
    // GET /Stores — all branches for both roles
    // -------------------------------------------------------
    public async Task<IActionResult> Index()
    {
        ViewData["ActivePage"] = "Stores";
        return View(await _storeService.GetAllActiveAsync());
    }

    // -------------------------------------------------------
    // GET /Stores/Details/3
    // -------------------------------------------------------
    public async Task<IActionResult> Details(int id)
    {
        ViewData["ActivePage"] = "Stores";
        var store = await _storeService.GetByIdAsync(id);
        if (store == null) { TempData["Error"] = "Branch not found."; return RedirectToAction(nameof(Index)); }
        return View(store);
    }

    // -------------------------------------------------------
    // GET /Stores/Create — HQ adds a new branch
    // -------------------------------------------------------
    [Authorize(Roles = "StoreManager")]
    public IActionResult Create()
    {
        ViewData["ActivePage"] = "Stores";
        return View(new StoreProfileEditViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "StoreManager")]
    public async Task<IActionResult> Create(StoreProfileEditViewModel vm)
    {
        ViewData["ActivePage"] = "Stores";
        if (!ModelState.IsValid) return View(vm);
        try
        {
            await _storeService.CreateStoreAsync(new Store
            {
                Name = vm.Name, Address = vm.Address,
                ContactPhone = vm.ContactPhone, ContactEmail = vm.ContactEmail, IsActive = true
            });
            TempData["Success"] = $"Branch '{vm.Name}' created!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating branch");
            TempData["Error"] = "An error occurred. Please try again.";
            return View(vm);
        }
    }

    // -------------------------------------------------------
    // GET /Stores/EditProfile/3 — HQ edits any branch
    // -------------------------------------------------------
    [Authorize(Roles = "StoreManager")]
    public async Task<IActionResult> EditProfile(int id)
    {
        ViewData["ActivePage"] = "Stores";
        var store = await _storeService.GetByIdAsync(id);
        if (store == null) { TempData["Error"] = "Branch not found."; return RedirectToAction(nameof(Index)); }
        return View(new StoreProfileEditViewModel
        {
            Id = store.Id, Name = store.Name, Address = store.Address,
            ContactPhone = store.ContactPhone, ContactEmail = store.ContactEmail
        });
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "StoreManager")]
    public async Task<IActionResult> EditProfile(StoreProfileEditViewModel vm)
    {
        ViewData["ActivePage"] = "Stores";
        if (!ModelState.IsValid) return View(vm);
        var store = await _storeService.GetByIdAsync(vm.Id);
        if (store == null) { TempData["Error"] = "Branch not found."; return RedirectToAction(nameof(Index)); }
        store.Name = vm.Name; store.Address = vm.Address;
        store.ContactPhone = vm.ContactPhone; store.ContactEmail = vm.ContactEmail;
        try
        {
            await _storeService.UpdateProfileAsync(store);
            TempData["Success"] = "Branch updated!";
            return RedirectToAction(nameof(Details), new { id = vm.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating branch {Id}", vm.Id);
            TempData["Error"] = "An error occurred.";
            return View(vm);
        }
    }

    // -------------------------------------------------------
    // GET /Stores/Inventory/3 — inventory for branch with id=3
    // -------------------------------------------------------
    [Authorize(Roles = "StoreManager")]
    public async Task<IActionResult> Inventory(int id)
    {
        ViewData["ActivePage"] = "Inventory";
        var store = await _storeService.GetByIdAsync(id);
        if (store == null) { TempData["Error"] = "Branch not found."; return RedirectToAction(nameof(Index)); }
        var inventory = await _storeService.GetInventoryAsync(id);
        ViewBag.Store = store;
        ViewBag.LowStockCount = inventory.Count(i => i.QuantityInStock <= i.LowStockThreshold);
        return View(inventory);
    }

    // -------------------------------------------------------
    // GET /Stores/AddProduct?storeId=3
    // -------------------------------------------------------
    [Authorize(Roles = "StoreManager")]
    public async Task<IActionResult> AddProduct(int storeId)
    {
        ViewData["ActivePage"] = "Inventory";
        var store = await _storeService.GetByIdAsync(storeId);
        if (store == null) { TempData["Error"] = "Branch not found."; return RedirectToAction(nameof(Index)); }
        var available = await _storeService.GetAvailableProductsToAddAsync(storeId);
        if (!available.Any()) { TempData["Success"] = "All products already tracked!"; return RedirectToAction(nameof(Inventory), new { id = storeId }); }
        ViewBag.ProductList = new SelectList(
            available.Select(p => new SelectListItem { Value = p.Id.ToString(), Text = $"{p.Supplier.CompanyName} — {p.Name} (PKR {p.UnitPrice:N0}/{p.Unit})" }),
            "Value", "Text");
        ViewBag.Store = store;
        return View(new AddInventoryItemViewModel { StoreId = storeId, LowStockThreshold = 10 });
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "StoreManager")]
    public async Task<IActionResult> AddProduct(AddInventoryItemViewModel vm)
    {
        ViewData["ActivePage"] = "Inventory";
        if (!ModelState.IsValid)
        {
            var store = await _storeService.GetByIdAsync(vm.StoreId);
            var available = await _storeService.GetAvailableProductsToAddAsync(vm.StoreId);
            ViewBag.ProductList = new SelectList(available.Select(p => new SelectListItem { Value = p.Id.ToString(), Text = $"{p.Supplier.CompanyName} — {p.Name} (PKR {p.UnitPrice:N0}/{p.Unit})" }), "Value", "Text");
            ViewBag.Store = store;
            return View(vm);
        }
        if (await _storeService.IsProductInInventoryAsync(vm.StoreId, vm.ProductId))
        { TempData["Error"] = "Product already tracked."; return RedirectToAction(nameof(Inventory), new { id = vm.StoreId }); }
        try
        {
            await _storeService.AddInventoryItemAsync(new InventoryItem { StoreId = vm.StoreId, ProductId = vm.ProductId, QuantityInStock = vm.QuantityInStock, LowStockThreshold = vm.LowStockThreshold });
            TempData["Success"] = "Product added to inventory!";
        }
        catch (Exception ex) { _logger.LogError(ex, "Error adding product to store {StoreId}", vm.StoreId); TempData["Error"] = "An error occurred."; }
        return RedirectToAction(nameof(Inventory), new { id = vm.StoreId });
    }

    // -------------------------------------------------------
    // GET /Stores/UpdateStock/7 — inventory item id=7
    // -------------------------------------------------------
    [Authorize(Roles = "StoreManager")]
    public async Task<IActionResult> UpdateStock(int id)
    {
        ViewData["ActivePage"] = "Inventory";
        var item = await _storeService.GetInventoryItemAsync(id);
        if (item == null) { TempData["Error"] = "Item not found."; return RedirectToAction(nameof(Index)); }
        ViewBag.StoreId = item.StoreId;
        return View(new UpdateStockViewModel
        {
            InventoryItemId = item.Id, ProductName = item.Product.Name,
            SupplierName = item.Product.Supplier.CompanyName,
            QuantityInStock = item.QuantityInStock, LowStockThreshold = item.LowStockThreshold
        });
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "StoreManager")]
    public async Task<IActionResult> UpdateStock(UpdateStockViewModel vm)
    {
        ViewData["ActivePage"] = "Inventory";
        if (!ModelState.IsValid) return View(vm);
        var item = await _storeService.GetInventoryItemAsync(vm.InventoryItemId);
        if (item == null) { TempData["Error"] = "Item not found."; return RedirectToAction(nameof(Index)); }
        try
        {
            await _storeService.UpdateInventoryItemAsync(new InventoryItem { Id = item.Id, StoreId = item.StoreId, ProductId = item.ProductId, QuantityInStock = vm.QuantityInStock, LowStockThreshold = vm.LowStockThreshold });
            TempData["Success"] = $"Stock updated for {vm.ProductName}.";
            return RedirectToAction(nameof(Inventory), new { id = item.StoreId });
        }
        catch (Exception ex) { _logger.LogError(ex, "Error updating stock {Id}", vm.InventoryItemId); TempData["Error"] = "An error occurred."; return View(vm); }
    }

    // -------------------------------------------------------
    // POST /Stores/RemoveProduct/7
    // -------------------------------------------------------
    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "StoreManager")]
    public async Task<IActionResult> RemoveProduct(int id)
    {
        var item = await _storeService.GetInventoryItemAsync(id);
        if (item == null) { TempData["Error"] = "Item not found."; return RedirectToAction(nameof(Index)); }
        var storeId = item.StoreId;
        try
        {
            await _storeService.RemoveInventoryItemAsync(id);
            TempData["Success"] = $"{item.Product.Name} removed from inventory.";
        }
        catch (Exception ex) { _logger.LogError(ex, "Error removing item {Id}", id); TempData["Error"] = "Cannot remove — may be linked to orders."; }
        return RedirectToAction(nameof(Inventory), new { id = storeId });
    }
}
