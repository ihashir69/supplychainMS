// ============================================================
// HomeController.cs — Dashboard with real live data
// ============================================================
//
// Queries the DB for counts and passes them to the dashboard view.
// Each role sees different stats relevant to their work.

using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupplyChainMS.Data;
using SupplyChainMS.Models;

namespace SupplyChainMS.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(ILogger<HomeController> logger, AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["ActivePage"] = "Dashboard";

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        ViewBag.UserName = user.FullName;
        ViewBag.UserRole = user.Role;

        // -------------------------------------------------------
        // SUPPLIER dashboard stats
        // -------------------------------------------------------
        if (User.IsInRole("Supplier"))
        {
            var supplier = await _context.Suppliers
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == user.Id);

            if (supplier != null)
            {
                // Orders waiting for confirmation
                ViewBag.PendingOrders = await _context.Orders
                    .CountAsync(o => o.SupplierId == supplier.Id && o.Status == OrderStatus.Submitted);

                // Orders confirmed but not yet shipped
                ViewBag.ConfirmedOrders = await _context.Orders
                    .CountAsync(o => o.SupplierId == supplier.Id && o.Status == OrderStatus.Confirmed);

                // Total active products
                ViewBag.ProductCount = await _context.Products
                    .CountAsync(p => p.SupplierId == supplier.Id && p.IsActive);

                // 5 most recent incoming orders for the activity feed
                ViewBag.RecentOrders = await _context.Orders
                    .Where(o => o.SupplierId == supplier.Id)
                    .Include(o => o.Store)
                    .OrderByDescending(o => o.CreatedAt)
                    .Take(5)
                    .AsNoTracking()
                    .ToListAsync();
            }
        }

        // -------------------------------------------------------
        // STORE MANAGER (HQ) dashboard stats
        // -------------------------------------------------------
        else if (User.IsInRole("StoreManager"))
        {
            // Total branches
            ViewBag.BranchCount = await _context.Stores.CountAsync(s => s.IsActive);

            // Inventory items at or below their low-stock threshold
            ViewBag.LowStockCount = await _context.InventoryItems
                .CountAsync(i => i.QuantityInStock <= i.LowStockThreshold);

            // Orders currently active (not fulfilled or cancelled)
            ViewBag.ActiveOrderCount = await _context.Orders
                .CountAsync(o => o.Status != OrderStatus.Fulfilled && o.Status != OrderStatus.Cancelled);

            // Orders waiting for supplier confirmation
            ViewBag.PendingSupplierCount = await _context.Orders
                .CountAsync(o => o.Status == OrderStatus.Submitted);

            // 5 most recent orders across all branches
            ViewBag.RecentOrders = await _context.Orders
                .Include(o => o.Store)
                .Include(o => o.Supplier)
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .AsNoTracking()
                .ToListAsync();

            // Branches with the most low-stock items (for alert summary)
            ViewBag.LowStockByStore = await _context.InventoryItems
                .Where(i => i.QuantityInStock <= i.LowStockThreshold)
                .Include(i => i.Store)
                .GroupBy(i => i.Store.Name)
                .Select(g => new { Store = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(3)
                .ToListAsync();
        }

        // -------------------------------------------------------
        // DRIVER dashboard stats
        // -------------------------------------------------------
        else if (User.IsInRole("Driver"))
        {
            var driver = await _context.Drivers
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.UserId == user.Id);

            if (driver != null)
            {
                ViewBag.PendingPickups = await _context.Shipments
                    .CountAsync(s => s.DriverId == driver.Id && s.Status == ShipmentStatus.Pending);

                ViewBag.InTransit = await _context.Shipments
                    .CountAsync(s => s.DriverId == driver.Id &&
                        (s.Status == ShipmentStatus.Dispatched || s.Status == ShipmentStatus.InTransit));

                ViewBag.DeliveredToday = await _context.Shipments
                    .CountAsync(s => s.DriverId == driver.Id &&
                        s.Status == ShipmentStatus.Delivered &&
                        s.ActualDeliveryDate.HasValue &&
                        s.ActualDeliveryDate.Value.Date == DateTime.UtcNow.Date);
            }
        }

        return View();
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
