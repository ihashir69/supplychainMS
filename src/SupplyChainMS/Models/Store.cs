// ============================================================
// Store.cs — A physical store/branch managed by HQ
// ============================================================
//
// Think KFC: HQ manages ALL branch locations centrally.
// The StoreManager role = HQ. They see ALL stores, not just one.
//
// ManagerUserId is nullable — it can record a branch contact person
// but does NOT restrict who manages this store.

namespace SupplyChainMS.Models;

public class Store
{
    public int Id { get; set; }

    // Optional contact person for this branch. NULL = managed directly by HQ.
    public string? ManagerUserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string ContactPhone { get; set; } = string.Empty;

    public string ContactEmail { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // -------------------------------------------------------
    // Navigation Properties
    // -------------------------------------------------------

    // Optional — null if no specific contact assigned
    public ApplicationUser? Manager { get; set; }

    public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
