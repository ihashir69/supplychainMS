// ============================================================
// ProductsController.cs — Handles all /Products/* URLs
// ============================================================
//
// Authorization model:
//   - SUPPLIER: full CRUD for their OWN products only
//   - STORE MANAGER: read-only (Index, Details) — can browse supplier catalogs
//   - DRIVER: no access to this controller
//
// A key security concern here is "ownership":
//   Before any edit/delete, we check that the product's SupplierId
//   matches the logged-in supplier's Id. Otherwise, a supplier could
//   edit another supplier's products by guessing the product Id.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SupplyChainMS.Models;
using SupplyChainMS.Services.Interfaces;
using SupplyChainMS.ViewModels;

namespace SupplyChainMS.Controllers;

[Authorize(Roles = "Supplier,StoreManager")]
public class ProductsController : Controller
{
    private readonly ISupplierService _supplierService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        ISupplierService supplierService,
        UserManager<ApplicationUser> userManager,
        ILogger<ProductsController> logger)
    {
        _supplierService = supplierService;
        _userManager = userManager;
        _logger = logger;
    }

    // -------------------------------------------------------
    // Helper: gets the logged-in supplier's profile.
    // Returns null if the user isn't a supplier or has no profile.
    // Used by multiple actions below to avoid repeating this code.
    // -------------------------------------------------------
    private async Task<Supplier?> GetCurrentSupplierAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return null;
        return await _supplierService.GetByUserIdAsync(user.Id);
    }

    // -------------------------------------------------------
    // GET /Products  or  GET /Products?supplierId=3
    // Supplier: sees their own products.
    // StoreManager: must provide a supplierId to browse that supplier's products.
    // -------------------------------------------------------
    public async Task<IActionResult> Index(int? supplierId)
    {
        ViewData["ActivePage"] = "Products";

        if (User.IsInRole("Supplier"))
        {
            // Suppliers only see their own products, ignoring any supplierId in the URL.
            var supplier = await GetCurrentSupplierAsync();
            if (supplier == null)
            {
                TempData["Error"] = "Supplier profile not found.";
                return RedirectToAction("Index", "Home");
            }

            var products = await _supplierService.GetProductsBySupplierIdAsync(supplier.Id);

            // Pass the supplier to the view so we can show "Khan Electronics > Products"
            ViewBag.Supplier = supplier;
            ViewBag.IsOwner = true;
            return View(products);
        }
        else
        {
            // StoreManager browsing a supplier's catalog.
            // They must provide a supplierId (e.g. from the Suppliers/Details page).
            if (supplierId == null)
            {
                TempData["Error"] = "Please select a supplier first.";
                return RedirectToAction("Index", "Suppliers");
            }

            var supplier = await _supplierService.GetByIdAsync(supplierId.Value);
            if (supplier == null)
            {
                TempData["Error"] = "Supplier not found.";
                return RedirectToAction("Index", "Suppliers");
            }

            var products = await _supplierService.GetProductsBySupplierIdAsync(supplierId.Value);
            ViewBag.Supplier = supplier;
            ViewBag.IsOwner = false;
            return View(products);
        }
    }

    // -------------------------------------------------------
    // GET /Products/Details/5
    // View a single product's full info.
    // Both roles can access this.
    // -------------------------------------------------------
    public async Task<IActionResult> Details(int id)
    {
        ViewData["ActivePage"] = "Products";

        var product = await _supplierService.GetProductByIdAsync(id);
        if (product == null)
        {
            TempData["Error"] = "Product not found.";
            return RedirectToAction(nameof(Index));
        }

        return View(product);
    }

    // -------------------------------------------------------
    // GET /Products/Create
    // Shows the "add new product" form.
    // ONLY suppliers can create products.
    // -------------------------------------------------------
    [Authorize(Roles = "Supplier")]
    public async Task<IActionResult> Create()
    {
        ViewData["ActivePage"] = "Products";

        var supplier = await GetCurrentSupplierAsync();
        if (supplier == null)
        {
            TempData["Error"] = "Supplier profile not found.";
            return RedirectToAction("Index", "Home");
        }

        // Pre-fill the SupplierId hidden field so the form knows who owns this product.
        var viewModel = new ProductCreateEditViewModel
        {
            SupplierId = supplier.Id
        };

        return View(viewModel);
    }

    // -------------------------------------------------------
    // POST /Products/Create
    // Receives the form, validates, inserts into DB.
    // -------------------------------------------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Supplier")]
    public async Task<IActionResult> Create(ProductCreateEditViewModel viewModel)
    {
        ViewData["ActivePage"] = "Products";

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        // Verify the SupplierId in the form matches the logged-in user's supplier.
        // Prevents someone from submitting a form with a tampered SupplierId.
        var supplier = await GetCurrentSupplierAsync();
        if (supplier == null || supplier.Id != viewModel.SupplierId)
        {
            TempData["Error"] = "Unauthorized action.";
            return RedirectToAction("Index", "Home");
        }

        // Map ViewModel → entity
        var product = new Product
        {
            SupplierId = supplier.Id,
            Name = viewModel.Name,
            Description = viewModel.Description,
            UnitPrice = viewModel.UnitPrice,
            Unit = viewModel.Unit,
            Category = viewModel.Category,
            StockQuantity = viewModel.StockQuantity,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            await _supplierService.AddProductAsync(product);
            TempData["Success"] = $"Product '{product.Name}' added successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product for supplier {SupplierId}", supplier.Id);
            TempData["Error"] = "An error occurred while saving. Please try again.";
            return View(viewModel);
        }
    }

    // -------------------------------------------------------
    // GET /Products/Edit/5
    // Shows the edit form for an existing product.
    // ONLY the supplier who OWNS this product can edit it.
    // -------------------------------------------------------
    [Authorize(Roles = "Supplier")]
    public async Task<IActionResult> Edit(int id)
    {
        ViewData["ActivePage"] = "Products";

        var supplier = await GetCurrentSupplierAsync();
        if (supplier == null)
        {
            TempData["Error"] = "Supplier profile not found.";
            return RedirectToAction("Index", "Home");
        }

        // Ownership check: is this product mine?
        var isOwner = await _supplierService.IsProductOwnedBySupplierAsync(id, supplier.Id);
        if (!isOwner)
        {
            TempData["Error"] = "Product not found or you don't have permission to edit it.";
            return RedirectToAction(nameof(Index));
        }

        var product = await _supplierService.GetProductByIdAsync(id);

        // Map entity → ViewModel for the form
        var viewModel = new ProductCreateEditViewModel
        {
            Id = product!.Id,
            SupplierId = product.SupplierId,
            Name = product.Name,
            Description = product.Description,
            UnitPrice = product.UnitPrice,
            Unit = product.Unit,
            Category = product.Category,
            StockQuantity = product.StockQuantity
        };

        return View(viewModel);
    }

    // -------------------------------------------------------
    // POST /Products/Edit/5
    // Saves the edited product.
    // -------------------------------------------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Supplier")]
    public async Task<IActionResult> Edit(int id, ProductCreateEditViewModel viewModel)
    {
        ViewData["ActivePage"] = "Products";

        // Make sure the id in the URL matches the id in the form.
        // Prevents URL/body mismatch attacks.
        if (id != viewModel.Id)
        {
            TempData["Error"] = "Invalid request.";
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var supplier = await GetCurrentSupplierAsync();
        if (supplier == null)
        {
            TempData["Error"] = "Supplier profile not found.";
            return RedirectToAction("Index", "Home");
        }

        // Ownership check again on POST (don't trust only the GET check).
        var isOwner = await _supplierService.IsProductOwnedBySupplierAsync(id, supplier.Id);
        if (!isOwner)
        {
            TempData["Error"] = "Unauthorized action.";
            return RedirectToAction(nameof(Index));
        }

        // Map ViewModel → entity
        var product = new Product
        {
            Id = viewModel.Id,
            SupplierId = supplier.Id,    // always use the DB value, not the form value
            Name = viewModel.Name,
            Description = viewModel.Description,
            UnitPrice = viewModel.UnitPrice,
            Unit = viewModel.Unit,
            Category = viewModel.Category,
            StockQuantity = viewModel.StockQuantity,
            IsActive = true
        };

        try
        {
            await _supplierService.UpdateProductAsync(product);
            TempData["Success"] = $"Product '{product.Name}' updated successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId}", id);
            TempData["Error"] = "An error occurred while saving. Please try again.";
            return View(viewModel);
        }
    }

    // -------------------------------------------------------
    // GET /Products/Delete/5
    // Shows a confirmation page before deleting.
    // -------------------------------------------------------
    [Authorize(Roles = "Supplier")]
    public async Task<IActionResult> Delete(int id)
    {
        ViewData["ActivePage"] = "Products";

        var supplier = await GetCurrentSupplierAsync();
        if (supplier == null)
        {
            TempData["Error"] = "Supplier profile not found.";
            return RedirectToAction("Index", "Home");
        }

        var isOwner = await _supplierService.IsProductOwnedBySupplierAsync(id, supplier.Id);
        if (!isOwner)
        {
            TempData["Error"] = "Product not found or you don't have permission.";
            return RedirectToAction(nameof(Index));
        }

        var product = await _supplierService.GetProductByIdAsync(id);
        return View(product);
    }

    // -------------------------------------------------------
    // POST /Products/Delete/5
    // Executes the deletion after the user confirms.
    // We use a different action name (DeleteConfirmed) to avoid
    // conflicting with GET /Products/Delete/5 — same URL, different methods.
    // -------------------------------------------------------
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Supplier")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var supplier = await GetCurrentSupplierAsync();
        if (supplier == null)
        {
            TempData["Error"] = "Supplier profile not found.";
            return RedirectToAction("Index", "Home");
        }

        var isOwner = await _supplierService.IsProductOwnedBySupplierAsync(id, supplier.Id);
        if (!isOwner)
        {
            TempData["Error"] = "Unauthorized action.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            // Get the product name before deleting (for the success message)
            var product = await _supplierService.GetProductByIdAsync(id);
            var productName = product?.Name ?? "Product";

            await _supplierService.DeleteProductAsync(id);
            TempData["Success"] = $"'{productName}' has been deleted.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {ProductId}", id);
            // This usually happens if the product is referenced by an order.
            TempData["Error"] = "Cannot delete this product — it may be linked to existing orders.";
        }

        return RedirectToAction(nameof(Index));
    }
}
