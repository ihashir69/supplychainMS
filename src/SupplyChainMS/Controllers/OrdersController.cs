// ============================================================
// OrdersController.cs — Handles all /Orders/* URLs
// ============================================================
//
// StoreManager = HQ. Sees all orders from all branches. Can create/submit.
// Supplier = External company. Sees only orders directed to them. Can confirm.

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

[Authorize(Roles = "Supplier,StoreManager")]
public class OrdersController : Controller
{
    private readonly IOrderService _orderService;
    private readonly ISupplierService _supplierService;
    private readonly IStoreService _storeService;
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService orderService, ISupplierService supplierService,
        IStoreService storeService, AppDbContext context, UserManager<ApplicationUser> userManager, ILogger<OrdersController> logger)
    {
        _context = context;
        _orderService = orderService;
        _supplierService = supplierService;
        _storeService = storeService;
        _userManager = userManager;
        _logger = logger;
    }

    private async Task<Supplier?> GetCurrentSupplierAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        return user == null ? null : await _supplierService.GetByUserIdAsync(user.Id);
    }

    private async Task PopulateCreateDropdownsAsync()
    {
        var stores = await _storeService.GetAllActiveAsync();
        var suppliers = await _supplierService.GetAllActiveAsync();
        ViewBag.StoreList = new SelectList(stores, "Id", "Name");
        ViewBag.SupplierList = new SelectList(suppliers, "Id", "CompanyName");
    }

    // -------------------------------------------------------
    // GET /Orders — HQ: all orders; Supplier: their incoming orders
    // -------------------------------------------------------
    public async Task<IActionResult> Index()
    {
        ViewData["ActivePage"] = "Orders";

        if (User.IsInRole("StoreManager"))
        {
            ViewBag.Role = "StoreManager";
            return View(await _orderService.GetAllOrdersAsync());
        }

        var supplier = await GetCurrentSupplierAsync();
        if (supplier == null) { TempData["Error"] = "Supplier profile not found."; return RedirectToAction("Index", "Home"); }
        ViewBag.Role = "Supplier";
        return View(await _orderService.GetOrdersBySupplierAsync(supplier.Id));
    }

    // -------------------------------------------------------
    // GET /Orders/Details/5
    // -------------------------------------------------------
    public async Task<IActionResult> Details(int id)
    {
        ViewData["ActivePage"] = "Orders";

        var order = await _orderService.GetByIdAsync(id);
        if (order == null) { TempData["Error"] = "Order not found."; return RedirectToAction(nameof(Index)); }

        if (User.IsInRole("Supplier"))
        {
            var supplier = await GetCurrentSupplierAsync();
            if (supplier == null || order.SupplierId != supplier.Id) { TempData["Error"] = "Access denied."; return RedirectToAction(nameof(Index)); }
        }

        return View(order);
    }

    // -------------------------------------------------------
    // GET /Orders/Create — HQ picks branch + supplier
    // -------------------------------------------------------
    [Authorize(Roles = "StoreManager")]
    public async Task<IActionResult> Create()
    {
        ViewData["ActivePage"] = "Orders";
        await PopulateCreateDropdownsAsync();
        return View(new OrderCreateViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "StoreManager")]
    public async Task<IActionResult> Create(OrderCreateViewModel viewModel)
    {
        ViewData["ActivePage"] = "Orders";
        if (!ModelState.IsValid) { await PopulateCreateDropdownsAsync(); return View(viewModel); }

        try
        {
            var created = await _orderService.CreateOrderAsync(new Order
            {
                StoreId = viewModel.StoreId, SupplierId = viewModel.SupplierId, Notes = viewModel.Notes
            });
            TempData["Success"] = "Order created! Now add products below.";
            return RedirectToAction(nameof(Details), new { id = created.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            await PopulateCreateDropdownsAsync();
            TempData["Error"] = "An error occurred.";
            return View(viewModel);
        }
    }

    // -------------------------------------------------------
    // GET /Orders/AddItem/5
    // -------------------------------------------------------
    [Authorize(Roles = "StoreManager")]
    public async Task<IActionResult> AddItem(int id)
    {
        ViewData["ActivePage"] = "Orders";
        var order = await _orderService.GetByIdAsync(id);
        if (order == null || order.Status != OrderStatus.Draft) { TempData["Error"] = "Order not editable."; return RedirectToAction(nameof(Index)); }

        var products = await _orderService.GetProductsForSupplierAsync(order.SupplierId);
        ViewBag.ProductList = new SelectList(
            products.Select(p => new { p.Id, Display = $"{p.Name} — PKR {p.UnitPrice:N0}/{p.Unit}" }),
            "Id", "Display");
        ViewBag.OrderId = id;
        ViewBag.SupplierName = order.Supplier.CompanyName;
        ViewBag.StoreName = order.Store.Name;
        return View(new AddOrderItemViewModel { OrderId = id, SupplierId = order.SupplierId });
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "StoreManager")]
    public async Task<IActionResult> AddItem(AddOrderItemViewModel viewModel)
    {
        ViewData["ActivePage"] = "Orders";
        var order = await _orderService.GetByIdAsync(viewModel.OrderId);
        if (order == null || order.Status != OrderStatus.Draft) { TempData["Error"] = "Order not editable."; return RedirectToAction(nameof(Index)); }

        if (!ModelState.IsValid)
        {
            var products = await _orderService.GetProductsForSupplierAsync(viewModel.SupplierId);
            ViewBag.ProductList = new SelectList(products.Select(p => new { p.Id, Display = $"{p.Name} — PKR {p.UnitPrice:N0}/{p.Unit}" }), "Id", "Display");
            ViewBag.OrderId = viewModel.OrderId; ViewBag.SupplierName = order.Supplier.CompanyName; ViewBag.StoreName = order.Store.Name;
            return View(viewModel);
        }

        try
        {
            await _orderService.AddOrderItemAsync(new OrderItem { OrderId = viewModel.OrderId, ProductId = viewModel.ProductId, Quantity = viewModel.Quantity });
            TempData["Success"] = "Item added.";
        }
        catch (Exception ex) { _logger.LogError(ex, "Error adding item"); TempData["Error"] = "Could not add item."; }

        return RedirectToAction(nameof(Details), new { id = viewModel.OrderId });
    }

    // -------------------------------------------------------
    // POST /Orders/RemoveItem/7
    // -------------------------------------------------------
    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "StoreManager")]
    public async Task<IActionResult> RemoveItem(int id)
    {
        var item = await _orderService.GetOrderItemAsync(id);
        if (item == null) { TempData["Error"] = "Item not found."; return RedirectToAction(nameof(Index)); }
        var orderId = item.OrderId;
        var order = await _orderService.GetByIdAsync(orderId);
        if (order == null || order.Status != OrderStatus.Draft) { TempData["Error"] = "Order not editable."; return RedirectToAction(nameof(Details), new { id = orderId }); }
        await _orderService.RemoveOrderItemAsync(id);
        TempData["Success"] = $"{item.Product.Name} removed.";
        return RedirectToAction(nameof(Details), new { id = orderId });
    }

    // -------------------------------------------------------
    // POST /Orders/Submit/5
    // -------------------------------------------------------
    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "StoreManager")]
    public async Task<IActionResult> Submit(int id)
    {
        var order = await _orderService.GetByIdAsync(id);
        if (order == null) { TempData["Error"] = "Order not found."; return RedirectToAction(nameof(Index)); }
        if (order.Status != OrderStatus.Draft) { TempData["Error"] = "Only Draft orders can be submitted."; return RedirectToAction(nameof(Details), new { id }); }
        if (!order.Items.Any()) { TempData["Error"] = "Add at least one product before submitting."; return RedirectToAction(nameof(Details), new { id }); }
        await _orderService.SubmitOrderAsync(id);
        TempData["Success"] = "Order submitted to supplier!";
        return RedirectToAction(nameof(Details), new { id });
    }

    // -------------------------------------------------------
    // POST /Orders/Confirm/5
    // -------------------------------------------------------
    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Supplier")]
    public async Task<IActionResult> Confirm(int id)
    {
        var order = await _orderService.GetByIdAsync(id);
        if (order == null) { TempData["Error"] = "Order not found."; return RedirectToAction(nameof(Index)); }
        var supplier = await GetCurrentSupplierAsync();
        if (supplier == null || order.SupplierId != supplier.Id) { TempData["Error"] = "Access denied."; return RedirectToAction(nameof(Index)); }
        if (order.Status != OrderStatus.Submitted) { TempData["Error"] = "Only Submitted orders can be confirmed."; return RedirectToAction(nameof(Details), new { id }); }
        await _orderService.ConfirmOrderAsync(id);
        TempData["Success"] = "Order confirmed! Arrange a shipment next.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // -------------------------------------------------------
    // GET /Orders/QuickRestock?storeId=1&productId=5
    // Called from the Inventory page "Order from Supplier" button.
    // Finds the product's supplier, then either:
    //   - Finds an existing Draft order for this store+supplier, or
    //   - Creates a new Draft order
    // Adds the product to that order (if not already there),
    // then redirects to the order's Details page to set quantity.
    // -------------------------------------------------------
    [Authorize(Roles = "StoreManager")]
    public async Task<IActionResult> QuickRestock(int storeId, int productId)
    {
        // Load the product to find its supplier
        var product = await _context.Products
            .Include(p => p.Supplier)
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product == null)
        {
            TempData["Error"] = "Product not found.";
            return RedirectToAction(nameof(Index));
        }

        // Check if there's already a Draft order for this store + supplier
        // If yes, reuse it (add the product there) rather than creating a duplicate
        var existingDraft = await _context.Orders
            .FirstOrDefaultAsync(o =>
                o.StoreId == storeId &&
                o.SupplierId == product.SupplierId &&
                o.Status == OrderStatus.Draft);

        Order order;
        if (existingDraft != null)
        {
            order = existingDraft;
        }
        else
        {
            // Create a new Draft order
            order = await _orderService.CreateOrderAsync(new Order
            {
                StoreId = storeId,
                SupplierId = product.SupplierId,
                Notes = $"Restock order — created from low stock alert"
            });
        }

        // Check if this product is already in the order; if not, add it with qty=1
        var alreadyInOrder = await _context.OrderItems
            .AnyAsync(i => i.OrderId == order.Id && i.ProductId == productId);

        if (!alreadyInOrder)
        {
            await _orderService.AddOrderItemAsync(new OrderItem
            {
                OrderId = order.Id,
                ProductId = productId,
                Quantity = 1    // HQ will adjust the quantity on the Details page
            });
        }

        TempData["Success"] = $"{product.Name} added to restock order. Review and set the quantity before submitting.";
        return RedirectToAction(nameof(Details), new { id = order.Id });
    }

    // -------------------------------------------------------
    // POST /Orders/Cancel/5
    // -------------------------------------------------------
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var order = await _orderService.GetByIdAsync(id);
        if (order == null) { TempData["Error"] = "Order not found."; return RedirectToAction(nameof(Index)); }

        if (User.IsInRole("StoreManager"))
        {
            if (order.Status != OrderStatus.Draft && order.Status != OrderStatus.Submitted)
            { TempData["Error"] = "This order can no longer be cancelled."; return RedirectToAction(nameof(Details), new { id }); }
        }
        else
        {
            var supplier = await GetCurrentSupplierAsync();
            if (supplier == null || order.SupplierId != supplier.Id) { TempData["Error"] = "Access denied."; return RedirectToAction(nameof(Index)); }
            if (order.Status != OrderStatus.Submitted) { TempData["Error"] = "Suppliers can only cancel Submitted orders."; return RedirectToAction(nameof(Details), new { id }); }
        }

        await _orderService.CancelOrderAsync(id);
        TempData["Success"] = "Order cancelled.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
