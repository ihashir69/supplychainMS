// ============================================================
// Product.cs — A product that a Supplier sells
// ============================================================
//
// Each product BELONGS TO one supplier.
// This is a "many-to-one" relationship:
//   Many products → belong to → one supplier
//
// The SupplierId property is the FOREIGN KEY that makes this link.
// EF Core uses it to do the JOIN automatically.

namespace SupplyChainMS.Models;

public class Product
{
    public int Id { get; set; }

    // Foreign Key → which supplier sells this product?
    public int SupplierId { get; set; }

    public string Name { get; set; } = string.Empty;

    // A longer description of the product (optional)
    public string? Description { get; set; }

    // Price per unit — "decimal" is used for money (never float/double for money!)
    // Reason: float has rounding errors. 0.1 + 0.2 = 0.30000000000000004 in float.
    // Decimal is exact for financial values.
    public decimal UnitPrice { get; set; }

    // What unit is this product sold in? (e.g., "kg", "box", "piece", "litre")
    public string Unit { get; set; } = "piece";

    // Category for grouping products (e.g., "Electronics", "Food", "Clothing")
    public string? Category { get; set; }

    // Stock available at the supplier's end
    public int StockQuantity { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // -------------------------------------------------------
    // Navigation Properties
    // -------------------------------------------------------

    // The supplier this product belongs to
    public Supplier Supplier { get; set; } = null!;

    // A product can appear in MANY order items across different orders
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    // A product can be tracked in inventory at MANY stores
    public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
}
