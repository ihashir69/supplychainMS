# Phase 1 — Project Setup & Foundation ✅ COMPLETE

**Branch:** `phase-1/setup`
**Date completed:** 2026-05-17
**Status:** Built, compiled (0 errors), database migrated, seed data working, app runs on localhost

---

## What Was Built

### 1. Project Structure
The app lives in `src/SupplyChainMS/`. Think of it like this:

```
src/SupplyChainMS/
├── Program.cs              ← App starts here (like main() in Java)
├── appsettings.json        ← Config file (safe to commit)
├── appsettings.Development.json  ← DB password (NEVER commit this)
│
├── Models/                 ← Database tables as C# classes
├── Data/                   ← Database connection + seed data
├── Controllers/            ← Handle HTTP requests (like Flask routes)
├── ViewModels/             ← Data shapes for forms/pages
├── Views/                  ← HTML templates (.cshtml files)
└── wwwroot/                ← Static files (CSS, JS, images)
```

---

## What Each File Does

### Models/ — The Database Tables

| File | What it represents | Key fields |
|------|--------------------|------------|
| `ApplicationUser.cs` | Login accounts | FullName, Role, Email, Password (hashed) |
| `Supplier.cs` | Supplier company profile | CompanyName, ContactEmail, Address |
| `Product.cs` | Products a supplier sells | Name, UnitPrice, StockQuantity, Category |
| `Store.cs` | A store managed by a StoreManager | Name, Address, ManagerUserId |
| `InventoryItem.cs` | How much of a product is in a store | StoreId, ProductId, QuantityInStock |
| `Order.cs` | A purchase order from Store → Supplier | Status, TotalAmount, StoreId, SupplierId |
| `OrderItem.cs` | One line in an order (product + qty) | OrderId, ProductId, Quantity, UnitPrice |
| `Shipment.cs` | Physical delivery of an order | Status, TrackingNumber, DriverId, OrderId |
| `DeliveryStatusLog.cs` | History of every status change | ShipmentId, Status, Timestamp, Notes |
| `Driver.cs` | Driver profile | FullName, VehiclePlate, VehicleType |
| `Message.cs` | Inbox message between users | SenderId, ReceiverId, Subject, Content |

### Data/ — Database Layer

**`AppDbContext.cs`** — The "gateway" to PostgreSQL.
- Contains `DbSet<T>` for every table (like a Python list that syncs with DB)
- `OnModelCreating()` defines all relationships (who has foreign key to whom)
- Extends `IdentityDbContext` which adds the Identity tables automatically

**`SeedData.cs`** — Populates the DB with test data on first run.
- Creates 3 roles: Supplier, StoreManager, Driver
- Creates 3 test users (one per role)
- Creates sample supplier profile, store, driver profile, and 3 products
- Runs automatically when the app starts via `Program.cs`

### Controllers/ — Request Handlers

**`AccountController.cs`** — Handles login/register/logout.
- `GET /Account/Login` → shows the login form
- `POST /Account/Login` → processes the form, creates the session cookie
- `GET /Account/Register` → shows the registration form
- `POST /Account/Register` → creates the user + assigns role + creates profile
- `POST /Account/Logout` → clears the cookie, redirects to login

**`HomeController.cs`** — Shows the dashboard after login.
- `GET /` → shows role-specific dashboard (different for Supplier/StoreManager/Driver)
- `[Authorize]` attribute — redirects to login if not logged in

### Views/ — HTML Templates

**`Views/Shared/_Layout.cshtml`** — The master template.
- Top navbar with user name, role badge, logout button
- Left sidebar with role-specific navigation links
- Flash message area (green for success, red for error)
- `@RenderBody()` — where each page's content goes

**`Views/Account/Login.cshtml`** — Login page with test credentials shown.
**`Views/Account/Register.cshtml`** — Registration form with role dropdown.
**`Views/Home/Index.cshtml`** — Dashboard with stat cards and quick action buttons.

---

## How It All Connects

```
Browser visits http://localhost:5226
        ↓
Program.cs routes the request
        ↓
HomeController.Index() is called
        ↓
[Authorize] checks: is user logged in? (reads cookie)
  → NO  → redirect to /Account/Login
  → YES → get user from DB, pass to View
        ↓
Views/Home/Index.cshtml renders
  → checks User.IsInRole("Supplier") etc.
  → shows the right dashboard cards
        ↓
_Layout.cshtml wraps the page
  → adds navbar + sidebar + flash messages
        ↓
HTML sent back to browser
```

---

## The Database (Supabase)

**Connection string location:** `appsettings.Development.json` (git-ignored)

**Tables created:**
- `AspNetUsers` — all user accounts (Identity managed)
- `AspNetRoles` — the 3 roles
- `AspNetUserRoles` — which user has which role
- `Suppliers`, `Products`, `Stores`, `InventoryItems`
- `Orders`, `OrderItems`
- `Shipments`, `DeliveryStatusLogs`
- `Drivers`, `Messages`
- `__EFMigrationsHistory` — tracks which migrations have run

---

## How to Run the App

```bash
# From: src/SupplyChainMS/

# One-time: apply DB migrations (already done)
dotnet ef database update

# Run the app
dotnet run

# OR run with auto-reload on code changes (better for development)
dotnet watch run
```

App opens at: **http://localhost:5226**

---

## Test Credentials

| Role | Email | Password | What they see |
|------|-------|----------|---------------|
| Supplier | supplier@test.com | Test@123 | Products, incoming orders, shipments |
| Store Manager | storemanager@test.com | Test@123 | Inventory, place orders, track deliveries |
| Driver | driver@test.com | Test@123 | Assigned deliveries |

---

## Phase 1 Checklist

- [x] Project compiles and runs
- [x] Database tables created in Supabase (15 tables)
- [x] Can register and login with 3 different roles
- [x] Sidebar layout renders correctly with role-based nav
- [x] Seed data populates on first run
- [x] appsettings.Development.json protected from git

---

## What's Next: Phase 2

**Branch to create:** `phase-2/supplier`
**What gets built:** Full CRUD for Suppliers and their Products
- SuppliersController (list, create, edit, delete)
- ProductsController (list, create, edit, delete — linked to supplier)
- SupplierService (business logic layer)
- Razor views with Bootstrap tables and forms
