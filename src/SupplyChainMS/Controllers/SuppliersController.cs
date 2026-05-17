// ============================================================
// SuppliersController.cs — Handles all /Suppliers/* URLs
// ============================================================
//
// This controller has TWO audiences:
//
//   1. SUPPLIER role users:
//      - Can view and edit their OWN profile
//      - Cannot see or edit other suppliers
//
//   2. STORE MANAGER role users:
//      - Can browse ALL active suppliers (read-only)
//      - Can view a supplier's details and their products
//      - Cannot edit anything
//
// Authorization rules:
//   [Authorize(Roles = "Supplier,StoreManager")] — both roles can access
//   [Authorize(Roles = "Supplier")]              — only suppliers can access
//
// Think of this controller as the "receptionist" for /Suppliers requests.
// It checks who you are, fetches data from the service, and hands it
// to the view to render as HTML.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SupplyChainMS.Models;
using SupplyChainMS.Services.Interfaces;
using SupplyChainMS.ViewModels;

namespace SupplyChainMS.Controllers;

[Authorize(Roles = "Supplier,StoreManager")]
public class SuppliersController : Controller
{
    // The service handles all database operations — controller stays thin.
    private readonly ISupplierService _supplierService;

    // UserManager lets us get the currently logged-in user's info.
    private readonly UserManager<ApplicationUser> _userManager;

    private readonly ILogger<SuppliersController> _logger;

    // ASP.NET injects all three of these automatically via Dependency Injection.
    public SuppliersController(
        ISupplierService supplierService,
        UserManager<ApplicationUser> userManager,
        ILogger<SuppliersController> logger)
    {
        _supplierService = supplierService;
        _userManager = userManager;
        _logger = logger;
    }

    // -------------------------------------------------------
    // GET /Suppliers
    // Shows different content depending on the role:
    //   - Supplier: redirected to their own profile
    //   - StoreManager: sees a list of all active suppliers
    // -------------------------------------------------------
    public async Task<IActionResult> Index()
    {
        ViewData["ActivePage"] = "Suppliers";

        if (User.IsInRole("Supplier"))
        {
            // Suppliers don't need a list — they only have ONE profile (themselves).
            // Redirect them to their own profile page.
            var user = await _userManager.GetUserAsync(User);
            var supplier = await _supplierService.GetByUserIdAsync(user!.Id);

            if (supplier == null)
            {
                // Edge case: somehow the supplier profile doesn't exist.
                TempData["Error"] = "Supplier profile not found. Please contact support.";
                return RedirectToAction("Index", "Home");
            }

            // Go directly to the supplier's own details page.
            return RedirectToAction(nameof(Details), new { id = supplier.Id });
        }

        // StoreManager: show the full supplier directory
        var suppliers = await _supplierService.GetAllActiveAsync();
        return View(suppliers);
    }

    // -------------------------------------------------------
    // GET /Suppliers/Details/5
    // Shows a supplier's profile and their product catalog.
    // Both roles can view this page.
    // -------------------------------------------------------
    public async Task<IActionResult> Details(int id)
    {
        ViewData["ActivePage"] = "Suppliers";

        var supplier = await _supplierService.GetByIdAsync(id);
        if (supplier == null)
        {
            TempData["Error"] = "Supplier not found.";
            return RedirectToAction(nameof(Index));
        }

        // If a Supplier is trying to view someone else's profile, redirect them.
        // A Supplier should only see their own profile.
        if (User.IsInRole("Supplier"))
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (supplier.UserId != currentUser!.Id)
            {
                TempData["Error"] = "You can only view your own profile.";
                return RedirectToAction(nameof(Index));
            }
        }

        return View(supplier);
    }

    // -------------------------------------------------------
    // GET /Suppliers/EditProfile
    // Shows the form to edit the supplier's own profile.
    // ONLY the Supplier role can do this.
    // -------------------------------------------------------
    [Authorize(Roles = "Supplier")]
    public async Task<IActionResult> EditProfile()
    {
        ViewData["ActivePage"] = "Suppliers";

        var user = await _userManager.GetUserAsync(User);
        var supplier = await _supplierService.GetByUserIdAsync(user!.Id);

        if (supplier == null)
        {
            TempData["Error"] = "Supplier profile not found.";
            return RedirectToAction("Index", "Home");
        }

        // Map the database entity → ViewModel.
        // We manually copy fields because we don't want to expose ALL fields
        // of the Supplier entity to the form (e.g., UserId, CreatedAt, Orders).
        var viewModel = new SupplierProfileEditViewModel
        {
            Id = supplier.Id,
            CompanyName = supplier.CompanyName,
            ContactEmail = supplier.ContactEmail,
            ContactPhone = supplier.ContactPhone,
            Address = supplier.Address,
            Description = supplier.Description
        };

        return View(viewModel);
    }

    // -------------------------------------------------------
    // POST /Suppliers/EditProfile
    // Receives the submitted form, validates it, saves to DB.
    // [ValidateAntiForgeryToken] prevents CSRF attacks
    // (a type of attack where another website tricks the user
    //  into submitting a form on your site without knowing).
    // -------------------------------------------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Supplier")]
    public async Task<IActionResult> EditProfile(SupplierProfileEditViewModel viewModel)
    {
        ViewData["ActivePage"] = "Suppliers";

        // ModelState.IsValid = did all the [Required], [EmailAddress] etc. validations pass?
        if (!ModelState.IsValid)
        {
            // Validation failed — return to the form with error messages shown.
            return View(viewModel);
        }

        // Double-check: make sure the Id in the form belongs to the current user.
        // This prevents a supplier from editing ANOTHER supplier's profile
        // by tampering with the hidden Id field.
        var user = await _userManager.GetUserAsync(User);
        var existingSupplier = await _supplierService.GetByUserIdAsync(user!.Id);

        if (existingSupplier == null || existingSupplier.Id != viewModel.Id)
        {
            TempData["Error"] = "Unauthorized action.";
            return RedirectToAction("Index", "Home");
        }

        // Map ViewModel → entity. Only update the fields the form controls.
        // We keep UserId, CreatedAt, IsActive, etc. untouched.
        existingSupplier.CompanyName = viewModel.CompanyName;
        existingSupplier.ContactEmail = viewModel.ContactEmail;
        existingSupplier.ContactPhone = viewModel.ContactPhone;
        existingSupplier.Address = viewModel.Address;
        existingSupplier.Description = viewModel.Description;

        try
        {
            await _supplierService.UpdateProfileAsync(existingSupplier);
            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction(nameof(Details), new { id = existingSupplier.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating supplier profile for user {UserId}", user.Id);
            TempData["Error"] = "An error occurred while saving. Please try again.";
            return View(viewModel);
        }
    }
}
