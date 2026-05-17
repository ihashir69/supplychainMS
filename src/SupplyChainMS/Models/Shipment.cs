// ============================================================
// Shipment.cs — The physical delivery of an order
// ============================================================
//
// Once a supplier confirms an order, they create a Shipment.
// The shipment is assigned to a Driver and tracks delivery progress.
//
// Shipment Status flow:
//   Pending → goods are packed but driver not assigned yet
//   Dispatched → driver picked up the goods, heading to store
//   InTransit → on the way
//   Delivered → successfully delivered to the store
//   Failed → delivery attempt failed (wrong address, nobody home, etc.)

namespace SupplyChainMS.Models;

public enum ShipmentStatus
{
    Pending,
    Dispatched,
    InTransit,
    Delivered,
    Failed
}

public class Shipment
{
    public int Id { get; set; }

    // Foreign Key → which order is being shipped?
    // One order has exactly one shipment (one-to-one relationship)
    public int OrderId { get; set; }

    // Foreign Key → which driver is assigned to deliver this?
    // Nullable because a driver might not be assigned immediately
    public int? DriverId { get; set; }

    public ShipmentStatus Status { get; set; } = ShipmentStatus.Pending;

    // A reference number the driver can use for tracking (e.g., "SHP-20240515-001")
    public string TrackingNumber { get; set; } = string.Empty;

    // When is the supplier planning to deliver?
    public DateTime? EstimatedDeliveryDate { get; set; }

    // When was it actually delivered? (null until confirmed delivered)
    public DateTime? ActualDeliveryDate { get; set; }

    // Any special instructions (e.g., "Call before arrival", "Loading dock B")
    public string? DeliveryNotes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // -------------------------------------------------------
    // Navigation Properties
    // -------------------------------------------------------

    public Order Order { get; set; } = null!;
    public Driver? Driver { get; set; }

    // ALL status changes are recorded here — this gives us a full history.
    // This is called "audit log" or "event sourcing" pattern.
    // Instead of overwriting the status, we add a new log entry each time.
    public ICollection<DeliveryStatusLog> StatusLogs { get; set; } = new List<DeliveryStatusLog>();
}
