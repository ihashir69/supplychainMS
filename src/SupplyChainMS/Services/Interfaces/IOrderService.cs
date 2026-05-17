// ============================================================
// IOrderService.cs — Contract for order operations
// ============================================================
//
// Order lifecycle:
//   Draft → Submitted → Confirmed → Fulfilled → Cancelled
//
//   Draft:     HQ created it, still adding products, not sent yet.
//   Submitted: Sent to supplier. Locked — no more item changes.
//   Confirmed: Supplier accepted it. Shipment will be created next.
//   Fulfilled: Delivered. Complete.
//   Cancelled: Cancelled by either party.

using SupplyChainMS.Models;

namespace SupplyChainMS.Services.Interfaces;

public interface IOrderService
{
    // Get one order with everything loaded: Store, Supplier, Items, Items.Product, Shipment
    Task<Order?> GetByIdAsync(int id);

    // All orders across all branches (HQ view)
    Task<List<Order>> GetAllOrdersAsync();

    // All orders placed by a specific store
    Task<List<Order>> GetOrdersByStoreAsync(int storeId);

    // All orders received by a supplier (their incoming orders)
    Task<List<Order>> GetOrdersBySupplierAsync(int supplierId);

    // Get a single order item (before editing or removing a line)
    Task<OrderItem?> GetOrderItemAsync(int orderItemId);

    // All active products from a specific supplier (for the Add Item dropdown)
    Task<List<Product>> GetProductsForSupplierAsync(int supplierId);

    // Create a new Draft order (header only — items added separately)
    Task<Order> CreateOrderAsync(Order order);

    // Add a product line to a Draft order; locks price at current value; recalculates total
    Task AddOrderItemAsync(OrderItem item);

    // Remove a product line from a Draft order; recalculates total
    Task RemoveOrderItemAsync(int orderItemId);

    // Draft → Submitted (HQ sends to supplier)
    Task SubmitOrderAsync(int orderId);

    // Submitted → Confirmed (supplier accepts)
    Task ConfirmOrderAsync(int orderId);

    // Cancel an order (either party, within allowed statuses)
    Task CancelOrderAsync(int orderId);

    // → Fulfilled (called by ShipmentService when delivery is complete)
    Task MarkFulfilledAsync(int orderId);
}
