// ============================================================
// InventoryItem.cs — Stock of a product at a specific store
// ============================================================
//
// This is a "junction table with extra data".
// It connects Store ↔ Product and also stores the quantity.
//
// Think of it like:
//   Store A has 50 units of Product X  → one InventoryItem row
//   Store A has 10 units of Product Y  → another InventoryItem row
//   Store B has 30 units of Product X  → another InventoryItem row

namespace SupplyChainMS.Models;

public class InventoryItem
{
    public int Id { get; set; }

    // Foreign Key → which store holds this stock?
    public int StoreId { get; set; }

    // Foreign Key → which product is it?
    public int ProductId { get; set; }

    // How many units are currently in stock at this store?
    public int QuantityInStock { get; set; } = 0;

    // When the stock falls below this number, show a "low stock" alert.
    // Default is 10 — the store manager can change this per product.
    public int LowStockThreshold { get; set; } = 10;

    // When was this inventory record last updated?
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // -------------------------------------------------------
    // Navigation Properties
    // -------------------------------------------------------

    public Store Store { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
