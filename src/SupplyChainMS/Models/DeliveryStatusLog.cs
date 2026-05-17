// ============================================================
// DeliveryStatusLog.cs — History of every status change on a shipment
// ============================================================
//
// This is an "append-only log" pattern.
// Every time a shipment's status changes, we ADD a new row here.
// We NEVER update or delete rows — we only INSERT.
//
// Why? Because it gives a complete audit trail:
//   2024-05-15 09:00 — Pending   (supplier packed goods)
//   2024-05-15 10:30 — Dispatched (driver picked up)
//   2024-05-15 14:15 — InTransit  (en route)
//   2024-05-15 16:45 — Delivered  (handed to store)
//
// This is like Git commits — you never rewrite history, you just add to it.

namespace SupplyChainMS.Models;

public class DeliveryStatusLog
{
    public int Id { get; set; }

    // Foreign Key → which shipment does this log entry belong to?
    public int ShipmentId { get; set; }

    // What was the new status at this point in time?
    public ShipmentStatus Status { get; set; }

    // When did this status change happen?
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // Optional comment explaining the status change
    // (e.g., "Package was damaged in transit, redelivery scheduled")
    public string? Notes { get; set; }

    // Who made this update? (their UserId)
    public string UpdatedByUserId { get; set; } = string.Empty;

    // -------------------------------------------------------
    // Navigation Properties
    // -------------------------------------------------------

    public Shipment Shipment { get; set; } = null!;

    // Who performed this update
    public ApplicationUser UpdatedByUser { get; set; } = null!;
}
