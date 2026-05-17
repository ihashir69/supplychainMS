# Phase 5 — Shipment & Delivery Tracking ✅ COMPLETE

**Branch:** `phase-3/store` (all phases are on this branch)
**Date completed:** 2026-05-17
**Status:** Built, compiled (0 errors)

---

## What Was Built

Shipment creation and delivery tracking. Once a supplier confirms an order, they
create a shipment, assign a driver, and the driver updates status as they deliver.
Every status change is permanently logged — full audit trail.

---

## Shipment Lifecycle

```
Pending → Dispatched → InTransit → Delivered
       ↘            ↘           ↘
      Failed        Failed       Failed
```

| Status | Who triggers | What it means |
|--------|------------|---------------|
| **Pending** | Supplier (auto on create) | Goods packed, driver not yet dispatched |
| **Dispatched** | Driver or Supplier | Driver picked up goods |
| **InTransit** | Driver | On the road to the branch |
| **Delivered** | Driver | Handed to branch; Order automatically marked Fulfilled |
| **Failed** | Driver or Supplier | Delivery attempt failed |

Only FORWARD transitions are allowed — you can't go backwards.
Delivered and Failed are terminal — no further updates.

---

## Status Log Pattern

Every status change adds a new row to `DeliveryStatusLogs`. Rows are NEVER updated
or deleted — only appended. This gives a complete timeline like Git commit history.

Each log entry records:
- The new status
- Timestamp
- Optional notes (e.g. "Delivered to branch manager Sara Ali")
- Who made the update (User.Id)

---

## New Files

### Services/Interfaces/IShipmentService.cs
```
GetByIdAsync     — full shipment with Order, Store, Supplier, Driver, all status logs
GetAllAsync      — all shipments (HQ view)
GetBySupplierAsync(supplierId) — supplier's shipments
GetByDriverAsync(driverId)     — driver's assigned deliveries
GetAvailableDriversAsync       — driver list for assignment dropdown
CreateAsync(shipment, userId)  — creates shipment + first "Pending" log entry
UpdateStatusAsync(id, status, userId, notes) — updates status + appends log + fulfills order if Delivered
AssignDriverAsync(shipmentId, driverId)
```

### Services/ShipmentService.cs
- Tracking number format: `SHP-YYYYMMDD-ORDERIDpadded` (e.g. `SHP-20260517-0003`)
- On `Delivered`: sets `ActualDeliveryDate` and calls `IOrderService.MarkFulfilledAsync`
- `AppDbContext` and `IOrderService` both injected (IOrderService for the Fulfill call)

### ViewModels/
- `ShipmentCreateViewModel` — OrderId, DriverId (optional), EstimatedDeliveryDate, DeliveryNotes
- `UpdateShipmentStatusViewModel` — ShipmentId, CurrentStatus (display), NewStatus (dropdown), Notes

### Controllers/ShipmentsController.cs
| Action | URL | Role | What |
|--------|-----|------|------|
| `Index` | GET /Shipments | All | HQ: all shipments; Supplier: theirs; Driver: assigned to them |
| `Details` | GET /Shipments/Details/5 | All | Full view with timeline |
| `Create` | GET/POST /Shipments/Create?orderId=3 | Supplier | Create shipment from Confirmed order |
| `UpdateStatus` | GET/POST /Shipments/UpdateStatus/5 | Driver + Supplier | Advance status; only valid next states shown |

### Views/Shipments/
| View | What it shows |
|------|--------------|
| `Index.cshtml` | Table with status summary chips; Update button on active shipments |
| `Details.cshtml` | Visual step progress bar (Pending→Dispatched→InTransit→Delivered); delivery info card; order items table; chronological status log with colored dots |
| `Create.cshtml` | Driver dropdown, estimated date picker, notes |
| `UpdateStatus.cshtml` | Only shows valid NEXT statuses (no backwards moves) |

### Orders/Details.cshtml updated
- "Create Shipment" button appears when order is Confirmed + no shipment exists (Supplier only)
- "Track Shipment SHP-xxx" link appears when shipment already exists

---

## Seed Data (5 shipments)

| Tracking # | Branch | Status | Notes |
|-----------|--------|--------|-------|
| SHP-...-0001 | Karachi Main | ✅ Delivered | Full log: Packed → Dispatched → InTransit → Delivered |
| SHP-...-0005 | Islamabad Blue Area | ✅ Delivered | Full log for office furniture order |
| SHP-...-0010 | Karachi Gulshan | ✅ Delivered | TechZone accessories |
| SHP-...-0002 | Lahore Defence | 🚚 Dispatched | In progress — driver en route |
| SHP-...-0007 | Karachi Clifton | ⏳ Pending | No driver assigned yet |

---

## How to Test

### As Supplier (`supplier@test.com`)
1. Sidebar → Orders → find a Confirmed order → click "Create Shipment"
2. Pick driver Bilal Raza, set estimated date, add notes → submit
3. Sidebar → Shipments → see the new shipment at Pending status
4. Click "Update Status" → advance to Dispatched

### As Driver (`driver@test.com`)
1. Sidebar → "My Deliveries" → see assigned shipments
2. Click "Update" on a Dispatched shipment → advance to InTransit
3. Click "Update" again → advance to Delivered
4. Check that the order is now marked Fulfilled (go to Orders → verify status)

### As StoreManager (HQ)
1. Sidebar → "Deliveries" → see all shipments across all branches
2. Status chips at the top show: Delivered: 3, Dispatched: 1, Pending: 1
3. Click any shipment → visual step progress bar + full status log

---

## What's Next: Phase 6

**Inbox / Messaging System** — users can send messages to each other,
optionally linked to an Order or Shipment for context.
