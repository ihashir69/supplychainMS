# Phase 1 — Project Setup & Foundation ✅ COMPLETE

**Branch:** `phase-1/setup`
**Date completed:** 2026-05-17
**Status:** Built, compiled (0 errors), database migrated, seed data working, app runs on localhost

---

## What You Need to Install (Prerequisites)

Before anyone can run this project on a new machine, they need these 4 things installed.

---

### 1. .NET 8 SDK
This is the compiler and runtime — without it nothing works. It's like installing Python itself.

**Download:** https://dotnet.microsoft.com/download/dotnet/8.0
- Click **.NET SDK** → **Windows x64 Installer**
- Run the `.exe`, click through the installer
- **Close and reopen** any terminal/VS Code after installing

**Verify it worked:**
```bash
dotnet --version
# Should print: 8.0.xxx
```

---

### 2. dotnet-ef (EF Core CLI tool)
This is the command-line tool for creating and applying database migrations.
Like `python manage.py` in Django — it's how you create and update database tables.

**Install it** (run once after installing .NET SDK):
```bash
dotnet tool install --global dotnet-ef
```

**Verify:**
```bash
dotnet ef --version
# Should print a version number
```

> Note: if `dotnet ef` is not found after installing, close and reopen your terminal.

---

### 3. Git
For cloning the repo and switching branches. You probably already have this.

**Download:** https://git-scm.com/download/win
- Use all default settings during install

**Verify:**
```bash
git --version
```

---

### 4. VS Code (recommended editor)
Any text editor works, but VS Code with the C# extension gives you autocomplete and error highlighting.

**Download:** https://code.visualstudio.com

**Install this VS Code extension:**
- Open VS Code → Extensions (Ctrl+Shift+X) → search **"C# Dev Kit"** → Install
- This gives you syntax highlighting, IntelliSense (autocomplete), and error squiggles

---

### Full Setup Steps (for a new machine from scratch)

```
Step 1: Install .NET 8 SDK         (link above)
Step 2: Install dotnet-ef tool      (command above)
Step 3: Install Git                 (link above)
Step 4: Install VS Code + C# Dev Kit extension
Step 5: Clone the repo
Step 6: Create appsettings.Development.json with Supabase connection string
Step 7: Run the app
```

#### Step 5 — Clone the repo
```bash
git clone https://github.com/ihashir69/supplychainMS.git
cd supplychainMS
```

#### Step 6 — Create the secret config file
This file is git-ignored (not on GitHub) because it contains the database password.
You must create it manually on every new machine.

Create this file: `src/SupplyChainMS/appsettings.Development.json`

Paste this content and fill in the Supabase details:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=YOUR_SUPABASE_HOST;Port=5432;Database=postgres;Username=postgres.YOUR_PROJECT_REF;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

Get the connection string from: **Supabase Dashboard → Project Settings → Database → Connection Pooling → Session mode**

#### Step 7 — Run the app
```bash
cd src/SupplyChainMS

# First time only — applies migrations to create all tables
dotnet ef database update

# Start the app (seed data runs automatically on first start)
dotnet run
```

Open browser at: **http://localhost:5226**

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
