// ============================================================
// OrderService.cs — Implementation of IOrderService
// ============================================================

using Microsoft.EntityFrameworkCore;
using SupplyChainMS.Data;
using SupplyChainMS.Models;
using SupplyChainMS.Services.Interfaces;

namespace SupplyChainMS.Services;

public class OrderService : IOrderService
{
    private readonly AppDbContext _context;

    public OrderService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        return await _context.Orders
            .Include(o => o.Store)
            .Include(o => o.Supplier)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Include(o => o.Shipment)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<List<Order>> GetAllOrdersAsync()
    {
        return await _context.Orders
            .Include(o => o.Store)
            .Include(o => o.Supplier)
            .Include(o => o.Items)
            .AsNoTracking()
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Order>> GetOrdersByStoreAsync(int storeId)
    {
        return await _context.Orders
            .Where(o => o.StoreId == storeId)
            .Include(o => o.Supplier)
            .Include(o => o.Items)
            .AsNoTracking()
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Order>> GetOrdersBySupplierAsync(int supplierId)
    {
        return await _context.Orders
            .Where(o => o.SupplierId == supplierId)
            .Include(o => o.Store)
            .Include(o => o.Items)
            .AsNoTracking()
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<OrderItem?> GetOrderItemAsync(int orderItemId)
    {
        return await _context.OrderItems
            .Include(i => i.Product)
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == orderItemId);
    }

    public async Task<List<Product>> GetProductsForSupplierAsync(int supplierId)
    {
        return await _context.Products
            .Where(p => p.SupplierId == supplierId && p.IsActive)
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<Order> CreateOrderAsync(Order order)
    {
        order.Status = OrderStatus.Draft;
        order.CreatedAt = DateTime.UtcNow;
        order.TotalAmount = 0;
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task AddOrderItemAsync(OrderItem item)
    {
        // Lock the price at the current supplier rate
        var product = await _context.Products.FindAsync(item.ProductId)
            ?? throw new InvalidOperationException("Product not found.");
        item.UnitPrice = product.UnitPrice;
        _context.OrderItems.Add(item);
        await _context.SaveChangesAsync();
        await RecalculateTotalAsync(item.OrderId);
    }

    public async Task RemoveOrderItemAsync(int orderItemId)
    {
        var item = await _context.OrderItems.FindAsync(orderItemId);
        if (item == null) return;
        var orderId = item.OrderId;
        _context.OrderItems.Remove(item);
        await _context.SaveChangesAsync();
        await RecalculateTotalAsync(orderId);
    }

    public async Task SubmitOrderAsync(int orderId)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null || order.Status != OrderStatus.Draft) return;
        order.Status = OrderStatus.Submitted;
        order.SubmittedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task ConfirmOrderAsync(int orderId)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null || order.Status != OrderStatus.Submitted) return;
        order.Status = OrderStatus.Confirmed;
        order.ConfirmedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task CancelOrderAsync(int orderId)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null || order.Status == OrderStatus.Fulfilled || order.Status == OrderStatus.Cancelled) return;
        order.Status = OrderStatus.Cancelled;
        await _context.SaveChangesAsync();
    }

    public async Task MarkFulfilledAsync(int orderId)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null || order.Status != OrderStatus.Confirmed) return;
        order.Status = OrderStatus.Fulfilled;
        order.FulfilledAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    private async Task RecalculateTotalAsync(int orderId)
    {
        var total = await _context.OrderItems
            .Where(i => i.OrderId == orderId)
            .SumAsync(i => i.Quantity * i.UnitPrice);
        var order = await _context.Orders.FindAsync(orderId);
        if (order != null)
        {
            order.TotalAmount = total;
            await _context.SaveChangesAsync();
        }
    }
}
