// ============================================================
// IShipmentService.cs — Contract for shipment operations
// ============================================================
//
// After a supplier CONFIRMS an order, they create a Shipment.
// The shipment is assigned to a Driver who updates status as they deliver.
//
// Status flow:
//   Pending    → Goods packed, driver not yet dispatched
//   Dispatched → Driver picked up goods, heading out
//   InTransit  → On the road to the store
//   Delivered  → Successfully handed to the store (also marks Order as Fulfilled)
//   Failed     → Delivery failed (wrong address, damaged goods, etc.)
//
// Every status change is LOGGED — we never overwrite, we append.
// This gives a full audit trail (like git commits for a delivery).

using SupplyChainMS.Models;

namespace SupplyChainMS.Services.Interfaces;

public interface IShipmentService
{
    // -------------------------------------------------------
    // READ
    // -------------------------------------------------------

    // Full shipment with Order, Store, Supplier, Driver, and all status logs
    Task<Shipment?> GetByIdAsync(int id);

    // All shipments — for HQ/StoreManager overview
    Task<List<Shipment>> GetAllAsync();

    // Shipments for orders belonging to this supplier
    Task<List<Shipment>> GetBySupplierAsync(int supplierId);

    // Shipments assigned to this driver
    Task<List<Shipment>> GetByDriverAsync(int driverId);

    // All available drivers (for the assignment dropdown when creating a shipment)
    Task<List<Driver>> GetAvailableDriversAsync();

    // -------------------------------------------------------
    // WRITE
    // -------------------------------------------------------

    // Create a shipment for a Confirmed order.
    // Automatically adds the first "Pending" log entry.
    Task<Shipment> CreateAsync(Shipment shipment, string createdByUserId);

    // Update shipment status and append a log entry.
    // If status = Delivered: also marks the linked Order as Fulfilled.
    // If status = Failed: records the failure reason in the log.
    Task UpdateStatusAsync(int shipmentId, ShipmentStatus newStatus, string updatedByUserId, string? notes);

    // Assign or reassign a driver to a Pending shipment
    Task AssignDriverAsync(int shipmentId, int driverId);
}
