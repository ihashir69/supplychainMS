// ============================================================
// ShipmentService.cs — Implementation of IShipmentService
// ============================================================

using Microsoft.EntityFrameworkCore;
using SupplyChainMS.Data;
using SupplyChainMS.Models;
using SupplyChainMS.Services.Interfaces;

namespace SupplyChainMS.Services;

public class ShipmentService : IShipmentService
{
    private readonly AppDbContext _context;
    private readonly IOrderService _orderService;

    // IOrderService is injected here so we can mark the order as Fulfilled
    // when a shipment is Delivered — keeps the responsibility in one place.
    public ShipmentService(AppDbContext context, IOrderService orderService)
    {
        _context = context;
        _orderService = orderService;
    }

    // -------------------------------------------------------
    // Get one shipment with everything loaded
    // -------------------------------------------------------
    public async Task<Shipment?> GetByIdAsync(int id)
    {
        return await _context.Shipments
            .Include(s => s.Order)
                .ThenInclude(o => o.Store)
            .Include(s => s.Order)
                .ThenInclude(o => o.Supplier)
            .Include(s => s.Order)
                .ThenInclude(o => o.Items)
                    .ThenInclude(i => i.Product)
            .Include(s => s.Driver)
            .Include(s => s.StatusLogs)
                .ThenInclude(l => l.UpdatedByUser)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    // -------------------------------------------------------
    // All shipments (HQ overview) — newest first
    // -------------------------------------------------------
    public async Task<List<Shipment>> GetAllAsync()
    {
        return await _context.Shipments
            .Include(s => s.Order).ThenInclude(o => o.Store)
            .Include(s => s.Order).ThenInclude(o => o.Supplier)
            .Include(s => s.Driver)
            .AsNoTracking()
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    // -------------------------------------------------------
    // Shipments for a supplier's orders
    // -------------------------------------------------------
    public async Task<List<Shipment>> GetBySupplierAsync(int supplierId)
    {
        return await _context.Shipments
            .Include(s => s.Order).ThenInclude(o => o.Store)
            .Include(s => s.Driver)
            .Where(s => s.Order.SupplierId == supplierId)
            .AsNoTracking()
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    // -------------------------------------------------------
    // Shipments assigned to a driver
    // -------------------------------------------------------
    public async Task<List<Shipment>> GetByDriverAsync(int driverId)
    {
        return await _context.Shipments
            .Include(s => s.Order).ThenInclude(o => o.Store)
            .Include(s => s.Order).ThenInclude(o => o.Supplier)
            .Where(s => s.DriverId == driverId)
            .AsNoTracking()
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    // -------------------------------------------------------
    // All drivers (for the dropdown when creating a shipment)
    // -------------------------------------------------------
    public async Task<List<Driver>> GetAvailableDriversAsync()
    {
        return await _context.Drivers
            .AsNoTracking()
            .OrderBy(d => d.FullName)
            .ToListAsync();
    }

    // -------------------------------------------------------
    // Create a shipment from a Confirmed order
    // -------------------------------------------------------
    public async Task<Shipment> CreateAsync(Shipment shipment, string createdByUserId)
    {
        shipment.Status = ShipmentStatus.Pending;
        shipment.CreatedAt = DateTime.UtcNow;

        // Generate a unique tracking number: SHP-YYYYMMDD-OrderId
        shipment.TrackingNumber = $"SHP-{DateTime.UtcNow:yyyyMMdd}-{shipment.OrderId:D4}";

        _context.Shipments.Add(shipment);
        await _context.SaveChangesAsync();

        // Add the first log entry — "Pending" with the creator's user ID
        _context.DeliveryStatusLogs.Add(new DeliveryStatusLog
        {
            ShipmentId = shipment.Id,
            Status = ShipmentStatus.Pending,
            Timestamp = DateTime.UtcNow,
            Notes = "Shipment created and goods are being prepared.",
            UpdatedByUserId = createdByUserId
        });
        await _context.SaveChangesAsync();

        return shipment;
    }

    // -------------------------------------------------------
    // Update shipment status + append log entry
    // -------------------------------------------------------
    public async Task UpdateStatusAsync(int shipmentId, ShipmentStatus newStatus, string updatedByUserId, string? notes)
    {
        var shipment = await _context.Shipments.FindAsync(shipmentId);
        if (shipment == null) return;

        shipment.Status = newStatus;

        // If delivered, record the actual delivery time
        if (newStatus == ShipmentStatus.Delivered)
            shipment.ActualDeliveryDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Append a log entry — NEVER update existing ones
        _context.DeliveryStatusLogs.Add(new DeliveryStatusLog
        {
            ShipmentId = shipmentId,
            Status = newStatus,
            Timestamp = DateTime.UtcNow,
            Notes = notes,
            UpdatedByUserId = updatedByUserId
        });
        await _context.SaveChangesAsync();

        // If delivered, mark the linked order as Fulfilled
        if (newStatus == ShipmentStatus.Delivered)
            await _orderService.MarkFulfilledAsync(shipment.OrderId);
    }

    // -------------------------------------------------------
    // Assign or reassign a driver
    // -------------------------------------------------------
    public async Task AssignDriverAsync(int shipmentId, int driverId)
    {
        var shipment = await _context.Shipments.FindAsync(shipmentId);
        if (shipment == null) return;
        shipment.DriverId = driverId;
        await _context.SaveChangesAsync();
    }
}
