# Phase 4 — Order Management ✅ COMPLETE

**Branch:** `phase-4/orders`
**Date completed:** 2026-05-17
**Status:** Built, compiled (0 errors)

---

## What Was Built

The core of the supply chain: HQ places orders to suppliers, suppliers confirm them,
and the order progresses through a full lifecycle.

Also includes the HQ architecture fix from Phase 3 (nullable ManagerUserId),
a live-data dashboard, and rich seed data across all 10 branches.

---

## Order Lifecycle

```
Draft → Submitted → Confirmed → Fulfilled
                 ↘            ↘
               Cancelled    Cancelled
```

| Status | Who can act | What happens |
|--------|------------|--------------|
| **Draft** | HQ (StoreManager) | Can add/remove products, write notes. Not sent yet. |
| **Submitted** | Supplier | Order sent — locked. Supplier sees it in their dashboard. |
| **Confirmed** | Supplier | Accepted. Shipment will be created next (Phase 5). |
| **Fulfilled** | System (Phase 5) | Shipment delivered — order complete. |
| **Cancelled** | HQ (Draft/Submitted) or Supplier (Submitted only) | Cancelled with reason. |

---

## New Files

### Services/Interfaces/IOrderService.cs
```
GetByIdAsync        — full order with Store, Supplier, Items, Shipment loaded
GetAllOrdersAsync   — all orders across all branches (HQ view)
GetOrdersByStoreAsync / GetOrdersBySupplierAsync
GetOrderItemAsync / GetProductsForSupplierAsync
CreateOrderAsync / AddOrderItemAsync / RemoveOrderItemAsync
SubmitOrderAsync / ConfirmOrderAsync / CancelOrderAsync / MarkFulfilledAsync
```

### Services/OrderService.cs
- `AddOrderItemAsync` locks the price at the time of ordering (copies from `Product.UnitPrice`)
- `RecalculateTotalAsync` — private method, called after every add/remove; uses SQL `SUM()`
- All status transitions check the current state before proceeding (no invalid transitions)

### ViewModels/
- `OrderCreateViewModel` — StoreId (branch selector) + SupplierId + Notes
- `AddOrderItemViewModel` — OrderId + SupplierId + ProductId + Quantity

### Controllers/OrdersController.cs
| Action | URL | Role | What |
|--------|-----|------|------|
| `Index` | GET /Orders | Both | HQ: all orders across all branches; Supplier: only their incoming |
| `Details` | GET /Orders/Details/5 | Both | Full order view + status timeline + action buttons |
| `Create` | GET/POST /Orders/Create | HQ only | Pick branch + supplier → creates Draft |
| `AddItem` | GET/POST /Orders/AddItem/5 | HQ only | Add product line to Draft |
| `RemoveItem` | POST /Orders/RemoveItem/7 | HQ only | Remove line from Draft |
| `Submit` | POST /Orders/Submit/5 | HQ only | Draft → Submitted; blocks if empty |
| `Confirm` | POST /Orders/Confirm/5 | Supplier only | Submitted → Confirmed |
| `Cancel` | POST /Orders/Cancel/5 | Both | Cancel within allowed states |

### Views/Orders/
| View | What it shows |
|------|--------------|
| `Index.cshtml` | Table of all orders with status badge chips as summary at top |
| `Create.cshtml` | Branch dropdown + Supplier dropdown + Notes |
| `Details.cshtml` | Order header, status timeline (Created/Submitted/Confirmed/Fulfilled), items table with totals, action buttons (contextual by role + status) |
| `AddItem.cshtml` | Product dropdown (filtered to the order's supplier) + quantity |

---

## Dashboard Updated (HomeController.cs + Home/Index.cshtml)

All stat cards now show **live data from the database** instead of `--`.

### StoreManager (HQ) Dashboard
| Card | What it shows |
|------|--------------|
| Total Branches | Count of active stores |
| Low Stock Items | Count of inventory items at/below threshold — red if > 0 |
| Awaiting Supplier | Count of Submitted orders (waiting for confirmation) — yellow if > 0 |
| Active Orders | Count of orders not yet Fulfilled or Cancelled |
| Recent Orders table | Last 5 orders across all branches with branch, supplier, total, status |

### Supplier Dashboard
| Card | What it shows |
|------|--------------|
| Awaiting Confirmation | Count of Submitted orders directed to this supplier |
| Confirmed Orders | Count of Confirmed orders (ready to ship) |
| Active Products | Count of active products in their catalog |
| Recent Orders feed | Last 5 orders from stores |

### Driver Dashboard
- Pending pickups / In Transit / Delivered Today (counts from Shipments table)

---

## Seed Data Summary (after Phase 4)

### Suppliers & Products
| Supplier | Login | Products |
|---------|-------|---------|
| Khan Electronics | supplier@test.com | USB-C Cable, HDMI Cable, Wireless Mouse, Mechanical Keyboard, USB Hub, Laptop Stand |
| Pak Office Supplies | supplier2@test.com | A4 Paper, Ballpoint Pens, Whiteboard Markers, Office Chair, Printer Ink |
| TechZone Accessories | supplier3@test.com | 24-inch Monitor, USB Headset, HD Webcam, Network Switch, Surge Protector, Cable Kit |

All passwords: **Test@123**

### 10 Sample Orders (all statuses represented)
| # | Branch | Supplier | Status |
|---|--------|---------|--------|
| 1 | Karachi Main | Khan Electronics | ✅ Fulfilled |
| 2 | Lahore Defence | Khan Electronics | 🔵 Confirmed |
| 3 | Lahore Defence | Pak Office Supplies | 🟡 Submitted |
| 4 | Islamabad F-10 | Khan Electronics | ⬜ Draft |
| 5 | Islamabad Blue Area | Pak Office Supplies | ✅ Fulfilled |
| 6 | Rawalpindi Saddar | Pak Office Supplies | 🔴 Cancelled |
| 7 | Karachi Clifton | TechZone Accessories | 🔵 Confirmed |
| 8 | Faisalabad | TechZone Accessories | 🟡 Submitted |
| 9 | Peshawar | Pak Office Supplies | ⬜ Draft |
| 10 | Karachi Gulshan | TechZone Accessories | ✅ Fulfilled |

---

## How to Test

### As StoreManager (HQ)
1. Login → Dashboard shows live counts (branches, low stock, orders, etc.)
2. Sidebar → Orders → see all 10 orders across all branches
3. Click "New Order" → pick a branch + supplier → Draft created
4. Add products from that supplier's catalog
5. Submit the order → status changes to Submitted
6. Check that Karachi Clifton order #7 can be seen (Confirmed status)
7. Check that order #6 (Cancelled) shows correct badge

### As Supplier
1. Login as `supplier@test.com` → Dashboard shows 1 Confirmed, products count
2. Login as `supplier2@test.com` → sees 2 incoming orders (order #3 Submitted, order #6 Cancelled)
3. Login as `supplier3@test.com` → sees order #7 (Confirmed) and #8 (Submitted)
4. On order #8: click "Confirm Order" → status changes to Confirmed
5. On Submitted orders: "Cancel Order" button visible

---

## What's Next: Phase 5

**Branch:** `phase-5/shipment`
**Depends on:** Phase 4

Shipment tracking: Supplier creates a shipment from a Confirmed order, assigns a driver,
and the driver updates delivery status through: Pending → Dispatched → InTransit → Delivered.
