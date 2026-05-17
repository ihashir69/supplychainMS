// ============================================================
// Store.cs — A physical store that places orders
// ============================================================
//
// A store is managed by one StoreManager (ApplicationUser).
// It holds inventory (products in stock) and places orders to suppliers.

namespace SupplyChainMS.Models;

public class Store
{
    public int Id { get; set; }

    // Foreign Key → which user manages this store?
    public string ManagerUserId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string ContactPhone { get; set; } = string.Empty;

    public string ContactEmail { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // -------------------------------------------------------
    // Navigation Properties
    // -------------------------------------------------------

    // The user who manages this store
    public ApplicationUser Manager { get; set; } = null!;

    // All inventory items (products + quantities) in this store
    public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();

    // All orders this store has placed
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
