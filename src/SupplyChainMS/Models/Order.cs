// ============================================================
// Order.cs — A purchase order from a Store to a Supplier
// ============================================================
//
// The ORDER is the core of the whole system.
// Flow: Store places order → Supplier confirms → Shipment created → Driver delivers
//
// An order has a STATUS that moves through stages (like a state machine):
//   Draft → the store started filling it but hasn't sent yet
//   Submitted → the store sent it to the supplier
//   Confirmed → the supplier accepted it
//   Fulfilled → goods have been shipped
//   Cancelled → either party cancelled

namespace SupplyChainMS.Models;

// This is an "enum" — a fixed list of allowed values.
// In Python: from enum import Enum; class OrderStatus(Enum): ...
// In Java:   public enum OrderStatus { DRAFT, SUBMITTED, ... }
public enum OrderStatus
{
    Draft,
    Submitted,
    Confirmed,
    Fulfilled,
    Cancelled
}

public class Order
{
    public int Id { get; set; }

    // Foreign Key → which store placed this order?
    public int StoreId { get; set; }

    // Foreign Key → which supplier is this order going to?
    public int SupplierId { get; set; }

    // Current status of the order
    public OrderStatus Status { get; set; } = OrderStatus.Draft;

    // Optional notes from the store (e.g., "urgent delivery needed")
    public string? Notes { get; set; }

    // Calculated total price (sum of all OrderItems)
    // This is stored for historical accuracy — prices can change later
    public decimal TotalAmount { get; set; } = 0;

    // When was the order first created?
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // When was it submitted to the supplier?
    public DateTime? SubmittedAt { get; set; }

    // When did the supplier confirm it?
    public DateTime? ConfirmedAt { get; set; }

    // When was the shipment marked as delivered?
    public DateTime? FulfilledAt { get; set; }

    // -------------------------------------------------------
    // Navigation Properties
    // -------------------------------------------------------

    public Store Store { get; set; } = null!;
    public Supplier Supplier { get; set; } = null!;

    // One order can have MANY line items (e.g., 5x Product A, 10x Product B)
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

    // One order has at most ONE shipment
    public Shipment? Shipment { get; set; }
}
