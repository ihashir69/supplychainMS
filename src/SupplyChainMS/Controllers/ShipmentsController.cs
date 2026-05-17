// ============================================================
// ShipmentsController.cs — Handles all /Shipments/* URLs
// ============================================================
//
// Three audiences:
//
//   SUPPLIER: Creates shipments from Confirmed orders, assigns drivers.
//
//   DRIVER:   Sees their assigned deliveries.
//             Updates status as they pick up and deliver.
//
//   STORE MANAGER (HQ): Read-only. Tracks all deliveries across branches.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SupplyChainMS.Data;
using SupplyChainMS.Models;
using SupplyChainMS.Services.Interfaces;
using SupplyChainMS.ViewModels;

namespace SupplyChainMS.Controllers;

[Authorize(Roles = "Supplier,StoreManager,Driver")]
public class ShipmentsController : Controller
{
    private readonly IShipmentService _shipmentService;
    private readonly ISupplierService _supplierService;
    private readonly IOrderService _orderService;
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ShipmentsController> _logger;

    public ShipmentsController(
        IShipmentService shipmentService,
        ISupplierService supplierService,
        IOrderService orderService,
        AppDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<ShipmentsController> logger)
    {
        _shipmentService = shipmentService;
        _supplierService = supplierService;
        _orderService = orderService;
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    // -------------------------------------------------------
    // GET /Shipments
    // HQ: all shipments | Supplier: their shipments | Driver: assigned to them
    // -------------------------------------------------------
    public async Task<IActionResult> Index()
    {
        ViewData["ActivePage"] = "Shipments";
        var user = await _userManager.GetUserAsync(User);

        if (User.IsInRole("StoreManager"))
        {
            ViewBag.Role = "StoreManager";
            return View(await _shipmentService.GetAllAsync());
        }

        if (User.IsInRole("Supplier"))
        {
            var supplier = await _supplierService.GetByUserIdAsync(user!.Id);
            if (supplier == null) { TempData["Error"] = "Supplier profile not found."; return RedirectToAction("Index", "Home"); }
            ViewBag.Role = "Supplier";
            return View(await _shipmentService.GetBySupplierAsync(supplier.Id));
        }

        // Driver
        var driverProfile = await GetCurrentDriverAsync();
        if (driverProfile == null) { TempData["Error"] = "Driver profile not found."; return RedirectToAction("Index", "Home"); }
        ViewBag.Role = "Driver";
        return View(await _shipmentService.GetByDriverAsync(driverProfile.Id));
    }

    // -------------------------------------------------------
    // GET /Shipments/Details/5
    // All roles can view — suppliers and HQ see any; driver sees theirs
    // -------------------------------------------------------
    public async Task<IActionResult> Details(int id)
    {
        ViewData["ActivePage"] = "Shipments";

        var shipment = await _shipmentService.GetByIdAsync(id);
        if (shipment == null) { TempData["Error"] = "Shipment not found."; return RedirectToAction(nameof(Index)); }

        // Drivers can only view shipments assigned to them
        if (User.IsInRole("Driver"))
        {
            var driver = await GetCurrentDriverAsync();
            if (driver == null || shipment.DriverId != driver.Id)
            { TempData["Error"] = "Access denied."; return RedirectToAction(nameof(Index)); }
        }

        return View(shipment);
    }

    // -------------------------------------------------------
    // GET /Shipments/Create?orderId=3
    // Supplier creates a shipment from a Confirmed order
    // -------------------------------------------------------
    [Authorize(Roles = "Supplier")]
    public async Task<IActionResult> Create(int orderId)
    {
        ViewData["ActivePage"] = "Shipments";

        var order = await _orderService.GetByIdAsync(orderId);
        if (order == null || order.Status != OrderStatus.Confirmed)
        { TempData["Error"] = "Order not found or not yet confirmed."; return RedirectToAction("Index", "Orders"); }

        // Security: make sure this order belongs to the logged-in supplier
        var user = await _userManager.GetUserAsync(User);
        var supplier = await _supplierService.GetByUserIdAsync(user!.Id);
        if (supplier == null || order.SupplierId != supplier.Id)
        { TempData["Error"] = "Access denied."; return RedirectToAction("Index", "Orders"); }

        // Check if a shipment already exists for this order
        if (order.Shipment != null)
        { TempData["Error"] = "A shipment already exists for this order."; return RedirectToAction(nameof(Details), new { id = order.Shipment.Id }); }

        var drivers = await _shipmentService.GetAvailableDriversAsync();
        ViewBag.DriverList = new SelectList(
            drivers.Select(d => new { d.Id, Display = $"{d.FullName} ({d.VehicleType} — {d.VehiclePlate})" }),
            "Id", "Display");

        return View(new ShipmentCreateViewModel
        {
            OrderId = orderId,
            StoreName = order.Store.Name,
            SupplierName = order.Supplier.CompanyName,
            EstimatedDeliveryDate = DateTime.Today.AddDays(3)
        });
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Supplier")]
    public async Task<IActionResult> Create(ShipmentCreateViewModel vm)
    {
        ViewData["ActivePage"] = "Shipments";

        if (!ModelState.IsValid)
        {
            var drivers = await _shipmentService.GetAvailableDriversAsync();
            ViewBag.DriverList = new SelectList(drivers.Select(d => new { d.Id, Display = $"{d.FullName} ({d.VehicleType} — {d.VehiclePlate})" }), "Id", "Display");
            return View(vm);
        }

        var user = await _userManager.GetUserAsync(User);

        try
        {
            var shipment = await _shipmentService.CreateAsync(new Shipment
            {
                OrderId = vm.OrderId,
                DriverId = vm.DriverId,
                EstimatedDeliveryDate = vm.EstimatedDeliveryDate,
                DeliveryNotes = vm.DeliveryNotes
            }, user!.Id);

            TempData["Success"] = $"Shipment {shipment.TrackingNumber} created!";
            return RedirectToAction(nameof(Details), new { id = shipment.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating shipment for order {OrderId}", vm.OrderId);
            TempData["Error"] = "An error occurred. Please try again.";
            return RedirectToAction("Details", "Orders", new { id = vm.OrderId });
        }
    }

    // -------------------------------------------------------
    // GET /Shipments/UpdateStatus/5
    // Driver updates their delivery status
    // -------------------------------------------------------
    [Authorize(Roles = "Driver,Supplier")]
    public async Task<IActionResult> UpdateStatus(int id)
    {
        ViewData["ActivePage"] = "Shipments";

        var shipment = await _shipmentService.GetByIdAsync(id);
        if (shipment == null) { TempData["Error"] = "Shipment not found."; return RedirectToAction(nameof(Index)); }

        // Driver can only update their own shipments
        if (User.IsInRole("Driver"))
        {
            var driver = await GetCurrentDriverAsync();
            if (driver == null || shipment.DriverId != driver.Id)
            { TempData["Error"] = "Access denied."; return RedirectToAction(nameof(Index)); }
        }

        // Build the list of valid NEXT statuses from the current one
        // You can't go backwards, and Delivered/Failed are terminal states
        var nextStatuses = GetAllowedNextStatuses(shipment.Status);
        ViewBag.StatusList = new SelectList(
            nextStatuses.Select(s => new { Value = (int)s, Text = s.ToString() }),
            "Value", "Text");

        return View(new UpdateShipmentStatusViewModel
        {
            ShipmentId = shipment.Id,
            TrackingNumber = shipment.TrackingNumber,
            StoreName = shipment.Order.Store.Name,
            CurrentStatus = shipment.Status
        });
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Driver,Supplier")]
    public async Task<IActionResult> UpdateStatus(UpdateShipmentStatusViewModel vm)
    {
        ViewData["ActivePage"] = "Shipments";

        var shipment = await _shipmentService.GetByIdAsync(vm.ShipmentId);
        if (shipment == null) { TempData["Error"] = "Shipment not found."; return RedirectToAction(nameof(Index)); }

        if (User.IsInRole("Driver"))
        {
            var driver = await GetCurrentDriverAsync();
            if (driver == null || shipment.DriverId != driver.Id)
            { TempData["Error"] = "Access denied."; return RedirectToAction(nameof(Index)); }
        }

        if (!ModelState.IsValid)
        {
            var nextStatuses = GetAllowedNextStatuses(shipment.Status);
            ViewBag.StatusList = new SelectList(nextStatuses.Select(s => new { Value = (int)s, Text = s.ToString() }), "Value", "Text");
            return View(vm);
        }

        var user = await _userManager.GetUserAsync(User);

        try
        {
            await _shipmentService.UpdateStatusAsync(vm.ShipmentId, vm.NewStatus, user!.Id, vm.Notes);
            TempData["Success"] = $"Status updated to {vm.NewStatus}.";
            return RedirectToAction(nameof(Details), new { id = vm.ShipmentId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating shipment status {ShipmentId}", vm.ShipmentId);
            TempData["Error"] = "An error occurred.";
            return RedirectToAction(nameof(Details), new { id = vm.ShipmentId });
        }
    }

    // -------------------------------------------------------
    // Helper: what statuses can come AFTER the current one?
    // -------------------------------------------------------
    private static List<ShipmentStatus> GetAllowedNextStatuses(ShipmentStatus current)
    {
        return current switch
        {
            ShipmentStatus.Pending    => new() { ShipmentStatus.Dispatched, ShipmentStatus.Failed },
            ShipmentStatus.Dispatched => new() { ShipmentStatus.InTransit, ShipmentStatus.Failed },
            ShipmentStatus.InTransit  => new() { ShipmentStatus.Delivered, ShipmentStatus.Failed },
            _ => new()   // Delivered and Failed are terminal — no next states
        };
    }

    // -------------------------------------------------------
    // Helper: get the current driver's profile from the DB
    // -------------------------------------------------------
    private async Task<Driver?> GetCurrentDriverAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return null;
        return await _context.Drivers.AsNoTracking().FirstOrDefaultAsync(d => d.UserId == user.Id);
    }
}
