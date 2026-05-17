# Project: SupplyChainMS — Supply Chain Management System

## 🎓 Context

This is a **university project** at the University of Karachi. The team has **3 members** but only **1 developer** (Hashir) who writes the code using **Claude Code**. The other 2 teammates will present specific modules. **Nobody on the team knows C# or .NET** — they are learning as they go.

-----

## ⚠️ CRITICAL INSTRUCTION FOR CLAUDE CODE

**EXPLAIN EVERYTHING.** Every file you create, every function you write, every pattern you use — add clear comments explaining:

1. **WHAT** this code does (in plain English, not jargon)
1. **WHY** we’re doing it this way (the reasoning behind the pattern)
1. **HOW** it connects to other parts of the system

Write comments as if teaching a beginner. Use analogies where helpful. The people reading this code have ZERO experience with C# or .NET. If you’d normally skip a comment because “it’s obvious” — write it anyway. Nothing is obvious to this team.

Example of the level of commenting expected:

```csharp
// A "Controller" in ASP.NET is like a waiter in a restaurant.
// The customer (browser) makes a request, the controller receives it,
// talks to the kitchen (database via services), and returns the response.
//
// This controller handles everything related to Suppliers.
// All URLs starting with /Suppliers will come here.
[Route("Suppliers")]
public class SuppliersController : Controller
{
    // This is "Dependency Injection" — instead of creating our own
    // database connection, ASP.NET gives us one automatically.
    // Think of it like: instead of building your own oven,
    // the restaurant provides one for you when you start your shift.
    private readonly ISupplierService _supplierService;
}
```

-----

## 🏗️ Tech Stack

|Layer         |Technology                         |Why We Chose It                                                |
|--------------|-----------------------------------|---------------------------------------------------------------|
|Backend + UI  |ASP.NET Core MVC + Razor Pages     |All-in-one (no separate frontend), professor prefers .NET      |
|Language      |C# (.NET 8)                        |Required for ASP.NET Core                                      |
|ORM           |Entity Framework Core + Npgsql     |Generates DB tables from C# classes, like SQLModel for Python  |
|Database      |PostgreSQL (Supabase — free hosted)|Free managed DB, no setup needed                               |
|CSS Framework |Bootstrap 5                        |Comes built-in with dotnet templates, responsive out of the box|
|Authentication|ASP.NET Identity + Cookie Auth     |Built-in, handles login/register/roles automatically           |
|Deployment    |Render.com (free tier)             |Free, auto-deploys from GitHub, detects .NET automatically     |

-----

## 📁 Project Structure

```
SupplyChainMS/
├── CLAUDE.md                          ← You are here (Claude Code reads this first)
├── README.md                          ← Project overview for GitHub
├── .gitignore                         ← Ignore bin/, obj/, .vs/, appsettings.*.json
├── docker-compose.yml                 ← Local dev with Postgres (optional)
├── render.yaml                        ← Render.com deployment config
│
└── src/
    └── SupplyChainMS/                 ← The main (and only) project
        ├── Program.cs                 ← App entry point — configures services, middleware, DB
        ├── appsettings.json           ← Config (DB connection string, app settings)
        ├── appsettings.Development.json ← Dev-only config (local Postgres or Supabase)
        ├── SupplyChainMS.csproj       ← Project file (like package.json for .NET)
        │
        ├── Models/                    ← Database entities (the "tables" as C# classes)
        │   ├── User.cs                ← Extends ASP.NET Identity user with Role field
        │   ├── Supplier.cs
        │   ├── Product.cs
        │   ├── Store.cs
        │   ├── InventoryItem.cs
        │   ├── Order.cs
        │   ├── OrderItem.cs
        │   ├── Shipment.cs
        │   ├── DeliveryStatusLog.cs
        │   ├── Driver.cs
        │   └── Message.cs
        │
        ├── Data/                      ← Database configuration
        │   ├── AppDbContext.cs         ← EF Core DbContext (defines all tables + relationships)
        │   └── SeedData.cs            ← Fake data for testing/demo
        │
        ├── Services/                  ← Business logic layer (sits between Controller and DB)
        │   ├── Interfaces/            ← Contracts (what each service CAN do)
        │   │   ├── ISupplierService.cs
        │   │   ├── IStoreService.cs
        │   │   ├── IOrderService.cs
        │   │   ├── IShipmentService.cs
        │   │   └── IMessageService.cs
        │   ├── SupplierService.cs
        │   ├── StoreService.cs
        │   ├── OrderService.cs
        │   ├── ShipmentService.cs
        │   └── MessageService.cs
        │
        ├── Controllers/               ← Handle HTTP requests, return Views
        │   ├── HomeController.cs       ← Landing page, dashboard
        │   ├── AccountController.cs    ← Login, Register, Logout
        │   ├── SuppliersController.cs  ← CRUD for suppliers
        │   ├── ProductsController.cs   ← Products linked to suppliers
        │   ├── StoresController.cs     ← Store management
        │   ├── OrdersController.cs     ← Place and manage orders
        │   ├── ShipmentsController.cs  ← Delivery tracking
        │   └── MessagesController.cs   ← Inbox system
        │
        ├── ViewModels/                ← Data shaped for the UI (not raw DB entities)
        │   ├── DashboardViewModel.cs
        │   ├── OrderCreateViewModel.cs
        │   ├── ShipmentTrackingViewModel.cs
        │   └── InboxViewModel.cs
        │
        ├── Views/                     ← Razor Pages (HTML + C# = dynamic pages)
        │   ├── Shared/
        │   │   ├── _Layout.cshtml     ← Master template (navbar, sidebar, footer)
        │   │   └── _LoginPartial.cshtml
        │   ├── Home/
        │   │   └── Index.cshtml       ← Dashboard (role-specific)
        │   ├── Account/
        │   │   ├── Login.cshtml
        │   │   └── Register.cshtml
        │   ├── Suppliers/
        │   │   ├── Index.cshtml       ← List all suppliers
        │   │   ├── Create.cshtml      ← Add new supplier form
        │   │   ├── Edit.cshtml        ← Edit supplier
        │   │   └── Details.cshtml     ← View one supplier
        │   ├── Products/
        │   ├── Stores/
        │   ├── Orders/
        │   ├── Shipments/
        │   └── Messages/
        │       ├── Inbox.cshtml       ← Message list
        │       └── Thread.cshtml      ← Conversation view
        │
        ├── wwwroot/                   ← Static files (CSS, JS, images)
        │   ├── css/
        │   │   └── site.css           ← Custom styles on top of Bootstrap
        │   ├── js/
        │   │   └── site.js            ← Custom JavaScript
        │   └── lib/                   ← Bootstrap, jQuery (auto-included by template)
        │
        └── Migrations/                ← EF Core migration files (auto-generated)
```

-----

## 🗃️ Database Schema

### Entity Relationships (ER Summary)

```
User (ASP.NET Identity)
  ├── Role: Supplier | StoreManager | Driver
  ├── has one → Supplier profile (if role = Supplier)
  ├── has one → Store assignment (if role = StoreManager)
  └── has one → Driver profile (if role = Driver)

Supplier
  ├── has many → Products
  └── has many → Orders (received from stores)

Store
  ├── has many → InventoryItems (stock of products)
  └── has many → Orders (placed to suppliers)

Order
  ├── belongs to → Store (who placed it)
  ├── belongs to → Supplier (who fulfills it)
  ├── has many → OrderItems (products + quantities)
  └── has one → Shipment (when fulfilled)

Shipment
  ├── belongs to → Order
  ├── belongs to → Driver (assigned)
  └── has many → DeliveryStatusLogs (history: Pending → Dispatched → In Transit → Delivered)

Message
  ├── belongs to → Sender (User)
  ├── belongs to → Receiver (User)
  └── optionally linked to → Order or Shipment (context)
```

### Status Enums

```csharp
// Order goes through these stages:
// Draft → Submitted → Confirmed → Fulfilled → Cancelled
public enum OrderStatus { Draft, Submitted, Confirmed, Fulfilled, Cancelled }

// Shipment tracking stages:
// Pending → Dispatched → InTransit → Delivered → Failed
public enum ShipmentStatus { Pending, Dispatched, InTransit, Delivered, Failed }
```

-----

## 🔐 Authentication & Roles

Three roles, each sees a different dashboard:

|Role            |Dashboard Shows                                    |Can Do                                                             |
|----------------|---------------------------------------------------|-------------------------------------------------------------------|
|**Supplier**    |Incoming orders, shipment statuses, unread messages|View orders, manage products, update shipments, message stores     |
|**StoreManager**|Inventory levels, order history, delivery tracking |Place orders, track deliveries, manage inventory, message suppliers|
|**Driver**      |Assigned shipments, delivery schedule              |View shipments, update delivery status, message store managers     |

Use ASP.NET Identity with cookie authentication. Role-based `[Authorize(Roles = "...")]` on controllers.

-----

## 🎨 UI/UX Guidelines

- **Bootstrap 5** for layout (grid, cards, tables, modals, navbars)
- **Sidebar navigation** for the main dashboard layout
- **Color scheme**: Blue primary (#0d6efd), success green for delivered, warning yellow for in-transit, danger red for failed/cancelled
- **DataTables or simple Bootstrap tables** for list views
- **Toast notifications** for success/error messages (Bootstrap toasts)
- **Mobile responsive** — Bootstrap handles this automatically
- **Loading states** — show spinners during form submissions

-----

## 📐 Coding Conventions

### C# Style

- File-scoped namespaces (e.g., `namespace SupplyChainMS.Models;` not `namespace SupplyChainMS.Models { }`)
- Async/await on ALL controller actions (return `Task<IActionResult>`)
- `ILogger<T>` for logging — NEVER `Console.WriteLine`
- Nullable reference types enabled
- Use `var` when the type is obvious from the right side
- PascalCase for public members, _camelCase for private fields

### Entity Framework Core

- Fluent API for relationships in `AppDbContext.OnModelCreating()` — no data annotations
- Always use `.AsNoTracking()` for read-only queries (better performance)
- Never expose entities directly to views — always use ViewModels

### Views (Razor)

- Use Tag Helpers (`asp-for`, `asp-action`, `asp-controller`) not raw HTML helpers
- Partial views for reusable components (e.g., `_OrderCard.cshtml`)
- Keep logic out of views — compute in controller, pass via ViewModel

-----

## ⚡ Commands

```bash
# Create the project (first time only)
dotnet new mvc -n SupplyChainMS -o src/SupplyChainMS

# Run the app (from src/SupplyChainMS/)
dotnet run

# Run with hot reload (auto-refreshes on code changes)
dotnet watch run

# Add a NuGet package (like pip install for .NET)
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore

# EF Core migrations (like Alembic for Python)
dotnet ef migrations add InitialCreate
dotnet ef database update

# If dotnet-ef tool not installed:
dotnet tool install --global dotnet-ef
```

-----

## 🔗 Database Connection (Supabase)

In `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=db.XXXXX.supabase.co;Database=postgres;Username=postgres;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true"
  }
}
```

In `Program.cs`:

```csharp
// This tells EF Core: "Use PostgreSQL, and here's the connection string"
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
```

**NEVER commit `appsettings.Development.json` to GitHub** — it contains your DB password. Add it to `.gitignore`.

-----

## 🚀 Deployment (Render.com)

### render.yaml (at repo root)

```yaml
services:
  - type: web
    name: supplychainms
    runtime: docker
    plan: free
    envVars:
      - key: ConnectionStrings__DefaultConnection
        sync: false  # Set manually in Render dashboard
      - key: ASPNETCORE_ENVIRONMENT
        value: Production
```

### Dockerfile (at repo root)

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/SupplyChainMS/SupplyChainMS.csproj", "SupplyChainMS/"]
RUN dotnet restore "SupplyChainMS/SupplyChainMS.csproj"
COPY src/SupplyChainMS/ SupplyChainMS/
WORKDIR "/src/SupplyChainMS"
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "SupplyChainMS.dll"]
```

### Deployment Steps

1. Push code to GitHub
1. Go to render.com → New → Web Service → Connect GitHub repo
1. Set environment variable `ConnectionStrings__DefaultConnection` = your Supabase connection string
1. Deploy — Render builds the Docker image and runs it automatically

-----

## 🔀 Git Branching Strategy

```
main                          ← Single integration + production branch
  │
  ├── phase-1/setup           ← Project scaffolding, DB, auth
  ├── phase-2/supplier        ← Supplier management module
  ├── phase-3/store           ← Store & inventory module
  ├── phase-4/orders          ← Order management module
  ├── phase-5/shipment        ← Shipment & delivery tracking
  ├── phase-6/inbox           ← Messaging system
  ├── phase-7/dashboard       ← Role-based dashboards
  ├── phase-8/deploy          ← Deployment config, final polish
  └── hotfix/*                ← Emergency fixes
```

### Branch Rules

- **NEVER push directly to `main`** — always merge from a `phase-*` branch via Pull Request
- Each phase branch is created from `main` and merged back into `main` when complete
- No `develop` branch — `main` is the single integration branch

-----

## 📋 Project Phases (Build Order)

### Phase 1: Project Setup & Foundation ← START HERE

**Branch:** `phase-1/setup`
**Depends on:** Nothing

```
Claude Code Prompt:
"Read CLAUDE.md. Scaffold the ASP.NET Core MVC project with:
- All entity models (Models/ folder) with detailed comments
- AppDbContext with Fluent API relationships
- ASP.NET Identity with 3 roles (Supplier, StoreManager, Driver)
- Cookie authentication with login/register pages
- Bootstrap 5 sidebar layout in _Layout.cshtml
- Npgsql connection to Supabase Postgres
- SeedData.cs with test users (one per role) and sample data
- Run the first EF Core migration
Explain every file and pattern as if teaching a beginner."
```

**Deliverables:**

- [ ] Project compiles and runs
- [ ] Database tables created in Supabase
- [ ] Can register and login with 3 different roles
- [ ] Sidebar layout renders correctly
- [ ] Seed data populates on first run

-----

### Phase 2: Supplier Management

**Branch:** `phase-2/supplier`
**Depends on:** Phase 1

```
Claude Code Prompt:
"Read CLAUDE.md. Build the Supplier Management module:
- SuppliersController with full CRUD (Index, Create, Edit, Details, Delete)
- ProductsController with CRUD (products belong to a supplier)
- ISupplierService + SupplierService for business logic
- Razor views with Bootstrap forms and tables
- Only Supplier role can manage their own products
- StoreManager role can VIEW suppliers and their products (read-only)
- Add supplier performance rating (average delivery score, calculate from shipment data later)
Explain every controller action and view thoroughly."
```

**Deliverables:**

- [ ] Supplier can add/edit/delete their products
- [ ] Store manager can browse supplier catalogs
- [ ] Proper authorization checks on every action

-----

### Phase 3: Store & Inventory Management

**Branch:** `phase-3/store`
**Depends on:** Phase 1, Phase 2

```
Claude Code Prompt:
"Read CLAUDE.md. Build the Store & Inventory module:
- StoresController — store profile management
- Inventory tracking per store (which products, how many in stock)
- Low stock alerts (highlight items below threshold in red)
- StoreManager can view their store's inventory
- Admin-style list of all stores
Explain the relationship between Store, InventoryItem, and Product clearly."
```

**Deliverables:**

- [ ] Store manager sees their inventory dashboard
- [ ] Low stock items highlighted
- [ ] Inventory quantities update correctly

-----

### Phase 4: Order Management ← CORE FEATURE

**Branch:** `phase-4/orders`
**Depends on:** Phase 2, Phase 3

```
Claude Code Prompt:
"Read CLAUDE.md. Build the Order Management module:
- OrdersController — create, view, confirm, cancel orders
- StoreManager creates an order → selects supplier → adds products + quantities
- Supplier sees incoming orders and can Confirm or Reject
- Order status flow: Draft → Submitted → Confirmed → Fulfilled → Cancelled
- OrderItems as a sub-table (one order can have multiple products)
- Order detail page showing all items, total, status timeline
Explain the full order lifecycle in comments."
```

**Deliverables:**

- [ ] Store manager can create and submit orders
- [ ] Supplier can view and confirm/reject orders
- [ ] Order status updates correctly
- [ ] Order detail page works

-----

### Phase 5: Shipment & Delivery Tracking

**Branch:** `phase-5/shipment`
**Depends on:** Phase 4

```
Claude Code Prompt:
"Read CLAUDE.md. Build the Shipment & Delivery Tracking module:
- ShipmentsController — create shipment from confirmed order, assign driver
- DeliveryStatusLog — every status change is recorded with timestamp
- Status flow: Pending → Dispatched → InTransit → Delivered → Failed
- Driver sees their assigned shipments
- Driver can update shipment status (e.g., mark as Delivered)
- Estimated vs actual delivery date tracking
- Shipment timeline view (show all status changes chronologically)
Explain how the status log pattern works (append-only history table)."
```

**Deliverables:**

- [ ] Supplier can create shipment and assign driver
- [ ] Driver sees assigned deliveries
- [ ] Driver updates delivery status
- [ ] Timeline view shows full status history

-----

### Phase 6: Inbox / Messaging System

**Branch:** `phase-6/inbox`
**Depends on:** Phase 1 (can be built in parallel with Phase 4/5)

```
Claude Code Prompt:
"Read CLAUDE.md. Build the Messaging/Inbox module:
- MessagesController — send message, view inbox, view thread
- Messages between any two users (Supplier ↔ StoreManager, StoreManager ↔ Driver)
- Messages can optionally be linked to an Order or Shipment (for context)
- Inbox view: list of conversations with unread count badge
- Thread view: chronological messages in a chat-like layout
- Unread message count in the sidebar navigation (like email)
Explain the message threading model in detail."
```

**Deliverables:**

- [ ] Users can send and receive messages
- [ ] Unread count shows in sidebar
- [ ] Messages link to relevant orders/shipments
- [ ] Chat-like thread view works

-----

### Phase 7: Dashboards & Polish

**Branch:** `phase-7/dashboard`
**Depends on:** ALL previous phases

```
Claude Code Prompt:
"Read CLAUDE.md. Build role-specific dashboards and polish the UI:
- Supplier Dashboard: pending orders count, active shipments, recent messages
- StoreManager Dashboard: inventory alerts, order status summary, recent deliveries
- Driver Dashboard: today's deliveries, pending pickups, messages
- Dashboard cards with icons and counts (Bootstrap cards)
- Quick action buttons (e.g., 'Create Order', 'Update Shipment')
- Breadcrumb navigation on all pages
- Toast notifications for all create/update/delete actions
- Form validation messages styled consistently
Make the dashboards visually impressive for a university presentation."
```

**Deliverables:**

- [ ] Each role sees a different, useful dashboard
- [ ] Navigation is intuitive with breadcrumbs
- [ ] UI is polished and presentation-ready

-----

### Phase 8: Deployment & Final

**Branch:** `phase-8/deploy`
**Depends on:** Phase 7

```
Claude Code Prompt:
"Read CLAUDE.md. Prepare the project for deployment:
- Dockerfile (multi-stage build for ASP.NET Core)
- render.yaml for Render.com
- Environment-based config (connection strings via env vars in production)
- Ensure EF Core runs migrations on startup in production
- Add a health check endpoint at /health
- README.md with setup instructions for teammates
- Final seed data for demo (realistic supplier names, products, orders)
Make sure the app works end-to-end when deployed."
```

**Deliverables:**

- [ ] App deployed and accessible via public URL
- [ ] Demo data loaded
- [ ] README has clear setup instructions

-----

## 📊 Phase Dependency Graph

```
Phase 1 (Setup)
  ├──→ Phase 2 (Suppliers)
  │       └──→ Phase 4 (Orders) ──→ Phase 5 (Shipments)
  ├──→ Phase 3 (Stores) ──┘
  ├──→ Phase 6 (Inbox)  ← Can be built in PARALLEL with 4 & 5
  │
  └──→ All phases merge into ──→ Phase 7 (Dashboards) ──→ Phase 8 (Deploy)
```

-----

## 👥 Presentation Assignment

|Teammate|Presents              |Modules to Understand                             |
|--------|----------------------|--------------------------------------------------|
|Person 1|Supplier + Products   |Phase 2 code, supplier dashboard, product catalog |
|Person 2|Orders + Shipments    |Phase 4-5 code, order lifecycle, delivery tracking|
|Person 3|Store + Inbox + Deploy|Phase 3, 6, 8 code, inventory, messaging, hosting |

Each person should be able to explain:

1. What their module does (business logic)
1. How the database tables relate
1. Walk through one user flow (e.g., “Store manager places an order”)

-----

## 🧪 Test Credentials (Seed Data)

|Role        |Email                |Password|
|------------|---------------------|--------|
|Supplier    |supplier@test.com    |Test@123|
|StoreManager|storemanager@test.com|Test@123|
|Driver      |driver@test.com      |Test@123|

-----

## 📝 Notes for Claude Code

- Always run `dotnet build` after creating/editing files to catch errors immediately
- If a migration fails, delete the last migration file and retry
- Use `dotnet watch run` during development for auto-reload
- When creating views, always start from the existing `_Layout.cshtml` template
- Keep controllers thin — complex logic goes in Services
- Every service method should be async and return `Task<T>`
- Use `TempData["Success"]` and `TempData["Error"]` for flash messages between redirects