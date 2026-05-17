// ============================================================
// OrderItem.cs — One line in an order (product + quantity)
// ============================================================
//
// Think of an order like a shopping cart receipt:
//   Order #101:
//     - 5 x Rice Bags @ $10 each    ← OrderItem row 1
//     - 3 x Cooking Oil @ $25 each  ← OrderItem row 2
//
// This is also called a "line item" or "order detail".
// It's a classic pattern in e-commerce and supply chain systems.

namespace SupplyChainMS.Models;

public class OrderItem
{
    public int Id { get; set; }

    // Foreign Key → which order does this line item belong to?
    public int OrderId { get; set; }

    // Foreign Key → which product is being ordered?
    public int ProductId { get; set; }

    // How many units are being ordered?
    public int Quantity { get; set; }

    // The price at the time of ordering — we COPY this from Product.UnitPrice.
    // Why copy it? Because the product's price might change later, but we need
    // this order's price to stay locked at what was agreed at order time.
    public decimal UnitPrice { get; set; }

    // Convenience property: Quantity × UnitPrice
    // This is NOT stored in DB (no column created).
    // It's calculated on-the-fly when we access it.
    // The [NotMapped] attribute would tell EF Core to ignore it — but since
    // we're using Fluent API config, we handle this there.
    public decimal LineTotal => Quantity * UnitPrice;

    // -------------------------------------------------------
    // Navigation Properties
    // -------------------------------------------------------

    public Order Order { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
