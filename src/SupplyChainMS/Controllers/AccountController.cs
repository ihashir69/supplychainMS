// ============================================================
// AccountController.cs — Handles Login, Register, Logout
// ============================================================
//
// A Controller in ASP.NET MVC is like a "route handler" in Flask/Express.
// Each public method = one URL route.
//
// Convention: for a class named "AccountController", the URL prefix is /Account
//   AccountController.Login()   → /Account/Login
//   AccountController.Register() → /Account/Register
//   AccountController.Logout()  → /Account/Logout
//
// HTTP verbs:
//   [HttpGet]  — when user VISITS the page (browser makes GET request)
//   [HttpPost] — when user SUBMITS a form (browser makes POST request)
//
// IActionResult = the "return type" of controller actions.
// It can be a View (HTML page), a Redirect, JSON, etc.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SupplyChainMS.Data;
using SupplyChainMS.Models;
using SupplyChainMS.ViewModels;

namespace SupplyChainMS.Controllers;

public class AccountController : Controller
{
    // These are "injected" by ASP.NET's DI system.
    // We declare what we NEED, and ASP.NET provides them automatically.
    // We never manually create these — the framework handles it.
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly AppDbContext _context;
    private readonly ILogger<AccountController> _logger;

    // Constructor: ASP.NET calls this and passes in the dependencies
    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        AppDbContext context,
        ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _logger = logger;
    }

    // -------------------------------------------------------
    // GET /Account/Login
    // Shows the login form
    // -------------------------------------------------------
    [HttpGet]
    [AllowAnonymous]   // anyone can access login page, even if not logged in
    public IActionResult Login(string? returnUrl = null)
    {
        // If already logged in, redirect to dashboard
        if (_signInManager.IsSignedIn(User))
            return RedirectToAction("Index", "Home");

        // Store the URL they were trying to visit before being redirected to login.
        // After successful login, we'll redirect them back there.
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    // -------------------------------------------------------
    // POST /Account/Login
    // Processes the login form submission
    // -------------------------------------------------------
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]  // prevents CSRF attacks (fake form submissions from other sites)
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        // ModelState.IsValid checks all [Required], [EmailAddress] etc. validations
        // If the form has errors, re-show it with error messages
        if (!ModelState.IsValid)
            return View(model);

        // PasswordSignInAsync attempts to log in.
        // Parameters: email, password, isPersistent (remember me), lockoutOnFailure
        var result = await _signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            _logger.LogInformation("User {Email} logged in.", model.Email);

            // If they were trying to visit a specific page, send them there.
            // Url.IsLocalUrl() prevents open redirect attacks (redirecting to external sites).
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        if (result.IsLockedOut)
        {
            _logger.LogWarning("User {Email} is locked out.", model.Email);
            ModelState.AddModelError(string.Empty, "Account is locked. Please try again in 5 minutes.");
            return View(model);
        }

        // Generic failure message — don't reveal whether the email exists or not
        ModelState.AddModelError(string.Empty, "Invalid email or password.");
        return View(model);
    }

    // -------------------------------------------------------
    // GET /Account/Register
    // Shows the registration form
    // -------------------------------------------------------
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        if (_signInManager.IsSignedIn(User))
            return RedirectToAction("Index", "Home");

        return View();
    }

    // -------------------------------------------------------
    // POST /Account/Register
    // Processes the registration form
    // -------------------------------------------------------
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // Validate that the selected role is one of our 3 allowed roles
        var allowedRoles = new[] { "Supplier", "StoreManager", "Driver" };
        if (!allowedRoles.Contains(model.Role))
        {
            ModelState.AddModelError("Role", "Please select a valid role.");
            return View(model);
        }

        // Create the user account
        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName,
            Role = model.Role,
            EmailConfirmed = true  // skip email verification for simplicity
        };

        // CreateAsync automatically hashes the password — NEVER store plain text!
        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            // Assign the role to the user
            await _userManager.AddToRoleAsync(user, model.Role);

            // Create the profile record based on role
            if (model.Role == "Supplier")
            {
                _context.Suppliers.Add(new Supplier
                {
                    UserId = user.Id,
                    CompanyName = model.FullName,  // they can update this later
                    ContactEmail = model.Email,
                    ContactPhone = string.Empty,
                    Address = string.Empty
                });
            }
            else if (model.Role == "StoreManager")
            {
                _context.Stores.Add(new Store
                {
                    ManagerUserId = user.Id,
                    Name = $"{model.FullName}'s Store",
                    Address = string.Empty,
                    ContactPhone = string.Empty,
                    ContactEmail = model.Email
                });
            }
            else if (model.Role == "Driver")
            {
                _context.Drivers.Add(new Driver
                {
                    UserId = user.Id,
                    FullName = model.FullName,
                    PhoneNumber = string.Empty,
                    VehiclePlate = string.Empty,
                    VehicleType = string.Empty
                });
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("New {Role} user registered: {Email}", model.Role, model.Email);

            // Automatically log them in after registration
            await _signInManager.SignInAsync(user, isPersistent: false);
            TempData["Success"] = $"Welcome, {model.FullName}! Your account has been created.";
            return RedirectToAction("Index", "Home");
        }

        // If user creation failed, add the errors to the form
        // (e.g., "Password must have at least one digit")
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    // -------------------------------------------------------
    // POST /Account/Logout
    // Signs the user out (clears their cookie)
    // -------------------------------------------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User logged out.");
        return RedirectToAction("Login");
    }

    // -------------------------------------------------------
    // GET /Account/AccessDenied
    // Shown when a logged-in user tries to access a page they don't have permission for
    // -------------------------------------------------------
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
