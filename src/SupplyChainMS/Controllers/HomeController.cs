// ============================================================
// HomeController.cs — Dashboard and landing page
// ============================================================
//
// [Authorize] = "you must be logged in to access this controller"
// If someone visits / while not logged in, they get redirected to /Account/Login
//
// For the AllowAnonymous routes (like Error), no login is needed.

using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupplyChainMS.Data;
using SupplyChainMS.Models;

namespace SupplyChainMS.Controllers;

[Authorize]  // all actions in this controller require login
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(
        ILogger<HomeController> logger,
        AppDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
    }

    // -------------------------------------------------------
    // GET /  or  GET /Home/Index
    // The dashboard — shows different content based on the user's role
    // -------------------------------------------------------
    public async Task<IActionResult> Index()
    {
        ViewData["ActivePage"] = "Dashboard";

        // Get the currently logged-in user
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        // Pass the user's name to the view
        ViewBag.UserName = user.FullName;
        ViewBag.UserRole = user.Role;

        return View();
    }

    // -------------------------------------------------------
    // Error page — shown on unhandled exceptions in production
    // [AllowAnonymous] because we might reach here before login
    // -------------------------------------------------------
    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
