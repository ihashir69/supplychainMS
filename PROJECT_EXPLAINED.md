# SupplyChainMS — The Complete Explanation

> A beginner-friendly, end-to-end walkthrough of **what this software is, what problem it
> solves, how it is built, and how every piece fits together.**
>
> This document assumes **zero C# / .NET experience**. Everything is explained in plain
> English, with analogies, and with the .NET jargon spelled out the first time it appears.
> If you can read code in Python or Java, you'll be fine here.

---

## Table of Contents

1. [What is this software?](#1-what-is-this-software)
2. [The problem it solves (and what most people get wrong)](#2-the-problem-it-solves-and-what-most-people-get-wrong)
3. [The tech stack — every tool and why we use it](#3-the-tech-stack--every-tool-and-why-we-use-it)
4. [Key .NET / ASP.NET terms you must know](#4-key-net--aspnet-terms-you-must-know)
5. [The big picture — how a request flows end to end](#5-the-big-picture--how-a-request-flows-end-to-end)
6. [The folder structure — what every folder is for](#6-the-folder-structure--what-every-folder-is-for)
7. [The architecture — the 4 layers](#7-the-architecture--the-4-layers)
8. [The database — tables and relationships](#8-the-database--tables-and-relationships)
9. [Authentication & roles — who can do what](#9-authentication--roles--who-can-do-what)
10. [The modules — feature by feature](#10-the-modules--feature-by-feature)
11. [Complete end-to-end story (a full user journey)](#11-complete-end-to-end-story-a-full-user-journey)
12. [How the app starts up (Program.cs line by line)](#12-how-the-app-starts-up-programcs-line-by-line)
13. [Running, building, and deploying](#13-running-building-and-deploying)
14. [Glossary — quick reference of every term](#14-glossary--quick-reference-of-every-term)

---

## 1. What is this software?

**SupplyChainMS** (Supply Chain Management System) is a **web application** that manages the
flow of goods from **suppliers → warehouses/stores → delivery drivers**, all in one place.

Picture a fast-food chain like **KFC**:

- A **head office (HQ)** runs many branch stores across different cities.
- Each branch needs stock (cups, packaging, electronics, office supplies, etc.).
- Branches **place orders** to **suppliers**.
- Suppliers **confirm** those orders and **ship** them.
- **Drivers** carry the shipment and **update its delivery status** along the way.
- Everyone can **message each other** inside the app to coordinate.

This app digitizes that entire cycle. Instead of phone calls, WhatsApp, and spreadsheets,
everything — orders, inventory, shipments, delivery tracking, and messaging — lives in a
single tracked system with a full history.

**It is a website**, not a phone app. It runs in a browser. The **same program produces
both the pages you see AND the logic behind them** (this is what "MVC + Razor" means — more
on that later).

---

## 2. The problem it solves (and what most people get wrong)

### The real-world problem

In a multi-branch business, the supply chain is usually a mess of disconnected tools:

| Pain point | What normally happens |
|---|---|
| **No single source of truth** | Orders live in WhatsApp, stock in Excel, deliveries in a driver's head. |
| **No history** | When a status changes, the old value is overwritten and lost. Nobody can prove "when did it ship?" |
| **No visibility** | The store doesn't know where its delivery is. The supplier doesn't know if the store got it. |
| **No accountability** | Who confirmed this order? Who marked it delivered? Unknown. |
| **Communication is off-system** | Messages about an order live in a chat app, disconnected from the order itself. |

### What this project does differently (the interesting part)

Most student projects (and many real apps) just build **CRUD** — Create, Read, Update,
Delete — screens and stop there. This project goes further in three ways that most people
**don't** bother to solve:

1. **Append-only status history (audit log).**
   When a shipment's status changes (Pending → Dispatched → InTransit → Delivered), we do
   **not** overwrite the old status. We **add a new row** to a `DeliveryStatusLog` table
   every time. This means you get a full, timestamped, tamper-evident **timeline** of what
   happened and when — like a courier tracking page. This is called the *event log* or
   *event sourcing* pattern.

2. **A proper state machine for orders.**
   An order can't just jump anywhere. It moves through defined stages
   (`Draft → Submitted → Confirmed → Fulfilled → Cancelled`), and each transition is
   timestamped (`SubmittedAt`, `ConfirmedAt`, `FulfilledAt`). This models the *lifecycle*
   of a business document, not just a row in a table.

3. **Context-linked messaging.**
   Messages can be **attached to a specific order or shipment**. So a conversation like
   "when will order #45 arrive?" is linked to order #45 itself — communication and data
   live together instead of being scattered.

Together these turn a simple database front-end into a system that **remembers, proves, and
connects** — which is what supply chains actually need.

---

## 3. The tech stack — every tool and why we use it

Think of building a web app like building a restaurant. You need a building, a kitchen, a
menu, waiters, a storage room, and a way to open to the public. Here's our "restaurant kit":

| Layer | Technology | What it does (plain English) | Why we chose it |
|---|---|---|---|
| **Language** | **C#** (pronounced "C-sharp") | The programming language everything is written in. Similar feel to Java. | Required for ASP.NET Core; strongly typed so errors are caught early. |
| **Runtime / Framework** | **.NET 8** | The "engine" that runs C# code. Like the JVM for Java or CPython for Python. | Latest long-term-support version; fast and cross-platform. |
| **Web framework** | **ASP.NET Core MVC + Razor** | Turns HTTP requests into web pages. Handles routing, forms, HTML rendering. | All-in-one: backend logic **and** the HTML pages come from the same project. No separate React/Angular frontend needed. |
| **ORM (database mapper)** | **Entity Framework Core (EF Core)** | Lets us talk to the database using C# objects instead of writing raw SQL. Like SQLModel/SQLAlchemy in Python or Hibernate in Java. | We define tables as C# classes; EF Core generates the SQL and the tables for us. |
| **Database driver** | **Npgsql** | The specific "adapter" that lets EF Core talk to PostgreSQL. | It's the standard PostgreSQL provider for .NET. |
| **Database** | **PostgreSQL** (hosted free on **Supabase**) | Where all the data actually lives (users, orders, shipments…). | Powerful, free, reliable; Supabase hosts it online so there's nothing to install. |
| **Authentication** | **ASP.NET Identity + Cookie Auth** | Handles register, login, logout, password hashing, and roles automatically. | Built into .NET; we don't have to hand-roll security (which is dangerous). |
| **CSS / UI framework** | **Bootstrap 5** | Ready-made buttons, tables, forms, navbars, and responsive layout. | Comes with the .NET template; looks professional with little effort; mobile-friendly for free. |
| **Deployment** | **Render.com** (free tier) + **Docker** | Puts the app online at a public URL. Docker packages the app so it runs identically anywhere. | Free, auto-deploys from GitHub, detects .NET automatically. |
| **Version control** | **Git + GitHub** | Tracks code history; each feature is built on its own branch. | Industry standard; enables the phase-by-phase branch workflow. |

### How the stack pieces connect (one sentence each)

- A user's **browser** sends a request.
- **ASP.NET Core MVC** receives it and routes it to a **Controller**.
- The controller calls a **Service** for business logic.
- The service uses **EF Core** to read/write data.
- **EF Core** speaks SQL through **Npgsql** to **PostgreSQL** on **Supabase**.
- The data comes back, a **Razor view** renders it into **HTML** styled by **Bootstrap**.
- The HTML is sent back to the browser.

---

## 4. Key .NET / ASP.NET terms you must know

These are the words you'll hear constantly. Learn these and the rest makes sense.

| Term | Plain-English meaning | Analogy |
|---|---|---|
| **.NET** | The whole platform/runtime that runs C# programs. | The JVM (Java) or the Python interpreter. |
| **ASP.NET Core** | The part of .NET for building web apps. | Flask/Django (Python) or Spring (Java). |
| **MVC** | **M**odel–**V**iew–**C**ontroller: a way to organize code so data, logic, and HTML are separated. | Model = the food, Controller = the waiter, View = the plate it's served on. |
| **Razor** | A file format (`.cshtml`) that mixes HTML with C#. Lets pages show live data. | Jinja2 templates (Python) or JSX, but for C#. |
| **Controller** | A C# class that handles incoming web requests for a topic (e.g. Orders). | The waiter who takes your order and brings your food. |
| **Action** | A single method inside a controller that answers one URL. | One specific thing the waiter can do ("bring the menu"). |
| **Model / Entity** | A C# class that represents one database table (e.g. `Order`). | A blueprint for one type of record. |
| **DbContext** | The single object that represents the whole database connection. | The door to the storage room; you ask it for anything in the DB. |
| **DbSet<T>** | One table, exposed as a list-like collection you can query. | One shelf in the storage room. |
| **EF Core** | Entity Framework Core — the ORM that maps C# classes ↔ database tables. | An automatic translator between C# and SQL. |
| **ORM** | Object-Relational Mapper: turns rows into objects and back. | A translator so you never write raw SQL. |
| **Migration** | A recorded, versioned change to the database structure. | A "commit" but for the shape of your database. |
| **Fluent API** | Configuring table relationships with chained method calls in code. | Describing how tables connect using sentences: "Order *has many* Items." |
| **Dependency Injection (DI)** | The framework automatically **gives** a class the objects it needs, instead of the class building them itself. | The restaurant provides the oven; the chef doesn't build one. |
| **Service** | A class holding business logic, sitting between controllers and the database. | The kitchen: does the real cooking, keeps the waiter simple. |
| **Interface** (`IOrderService`) | A contract listing what a service *can do*, without saying *how*. | A menu of capabilities; the actual class is the chef who fulfills them. |
| **ViewModel** | A class shaped specifically for one screen (not a raw DB table). | A plated dish arranged for the customer, not the raw ingredients. |
| **Middleware** | Steps every request passes through in order (auth check, routing…). | Security checkpoints at an airport, one after another. |
| **Tag Helper** | Special HTML attributes (`asp-for`, `asp-action`) that C# fills in. | Smart form fields that wire themselves to your data. |
| **Async / await** | Lets the app handle other requests while waiting on the database. | A waiter serving other tables while the kitchen cooks. |
| **Scoped / Singleton / Transient** | How long a DI-provided object lives (per request / forever / every time). | Scoped = one waiter per customer visit. |
| **Cookie authentication** | After login, the browser holds a small token so the server remembers you. | A wristband proving you already paid at the entrance. |
| **`[Authorize]`** | An attribute that blocks a page unless the user is logged in / has a role. | A "staff only" sign on a door. |
| **`TempData`** | A one-time message that survives a page redirect (e.g. "Saved!"). | A sticky note passed to the next page, then thrown away. |
| **`wwwroot`** | The public folder for static files (CSS, JS, images) served as-is. | The front lobby anyone can walk into. |
| **NuGet** | .NET's package manager (installs libraries). | `pip` (Python) or `npm` (JavaScript). |

---

## 5. The big picture — how a request flows end to end

Here is what happens the moment a Store Manager clicks **"Place Order"** in their browser:

```
 ┌──────────┐   1. HTTP request      ┌─────────────────────────────────────────┐
 │  BROWSER │ ─────────────────────▶ │            ASP.NET CORE APP              │
 │ (Chrome) │   POST /Orders/Create  │                                          │
 └────┬─────┘                        │  2. MIDDLEWARE PIPELINE (in order):      │
      │                              │     HTTPS → Static files → Routing →     │
      │                              │     Authentication → Authorization       │
      │                              │              │                           │
      │                              │              ▼                           │
      │                              │  3. CONTROLLER  (OrdersController)        │
      │                              │     "Which action matches this URL?"      │
      │                              │              │                           │
      │                              │              ▼                           │
      │                              │  4. SERVICE  (OrderService)               │
      │                              │     business logic: validate, total up   │
      │                              │              │                           │
      │                              │              ▼                           │
      │                              │  5. EF CORE  (AppDbContext)               │
      │                              │     translate C# → SQL                    │
      │                              └──────────────┼───────────────────────────┘
      │                                             ▼
      │                              ┌──────────────────────────┐
      │                              │  PostgreSQL on Supabase   │
      │                              │  (data is saved/loaded)   │
      │                              └──────────────┬───────────┘
      │                                             │ 6. rows come back
      │                              ┌──────────────▼───────────────────────────┐
      │  8. HTML page (styled by     │  7. RAZOR VIEW renders HTML using the     │
      │◀──── Bootstrap) returned ────│     data (via a ViewModel)                │
      │                              └───────────────────────────────────────────┘
```

**The golden rule of this architecture:** each layer only talks to its neighbor.
Controllers never touch the database directly — they ask a Service. Services never render
HTML — they return data. Views never contain business logic — they just display. This
separation is *why* the code stays understandable as it grows.

---

## 6. The folder structure — what every folder is for

The whole app lives in `src/SupplyChainMS/`. Here's every folder and its job:

```
src/SupplyChainMS/
│
├── Program.cs              ← THE STARTING POINT. Configures the app, DB, auth,
│                             services, and the request pipeline. Runs first.
│
├── appsettings.json        ← Public configuration (logging levels, etc.).
├── appsettings.Development.json  ← SECRET config (DB connection string/password).
│                                   NEVER committed to GitHub (.gitignore blocks it).
├── SupplyChainMS.csproj    ← The project file. Lists NuGet packages & settings.
│                             (Like package.json for Node or requirements.txt for Python.)
│
├── Models/                 ← THE DATABASE TABLES, as C# classes ("entities").
│   ├── ApplicationUser.cs      A person who logs in (extends Identity's user).
│   ├── Supplier.cs             A company that supplies goods.
│   ├── Product.cs              An item a supplier sells.
│   ├── Store.cs                A branch location managed by HQ.
│   ├── InventoryItem.cs        How much of a product a store currently stocks.
│   ├── Order.cs                A purchase order (Store → Supplier) + its status enum.
│   ├── OrderItem.cs            One line inside an order (product + quantity + price).
│   ├── Shipment.cs             The physical delivery of an order + its status enum.
│   ├── DeliveryStatusLog.cs    ONE entry in a shipment's status history (append-only).
│   ├── Driver.cs               A delivery driver's profile.
│   ├── Message.cs              An inbox message between two users.
│   └── ErrorViewModel.cs       Tiny helper for the error page.
│
├── Data/                   ← DATABASE WIRING.
│   ├── AppDbContext.cs         The gateway to the DB. Lists all tables (DbSets) and
│   │                            defines every relationship via the Fluent API.
│   └── SeedData.cs             Fills the DB with realistic demo data on first run
│                                (users, suppliers, 17 products, 10 branches, orders).
│
├── Services/               ← BUSINESS LOGIC (the "kitchen").
│   ├── Interfaces/             Contracts — WHAT each service can do.
│   │   ├── ISupplierService.cs
│   │   ├── IStoreService.cs
│   │   ├── IOrderService.cs
│   │   ├── IShipmentService.cs
│   │   └── IMessageService.cs
│   ├── SupplierService.cs      HOW supplier/product logic works.
│   ├── StoreService.cs         HOW store & inventory logic works.
│   ├── OrderService.cs         HOW the order lifecycle works.
│   ├── ShipmentService.cs      HOW shipment creation & status updates work.
│   └── MessageService.cs       HOW the inbox (conversations, unread counts) works.
│
├── Controllers/            ← REQUEST HANDLERS (the "waiters"). One per topic.
│   ├── HomeController.cs        Landing page + role-based dashboard.
│   ├── AccountController.cs     Register, Login, Logout, Access Denied.
│   ├── SuppliersController.cs   Supplier profiles + browsing catalogs.
│   ├── ProductsController.cs    Product CRUD (a supplier's items).
│   ├── StoresController.cs      Stores + inventory management.
│   ├── OrdersController.cs      The order lifecycle.
│   ├── ShipmentsController.cs   Delivery tracking.
│   └── MessagesController.cs    The inbox.
│
├── ViewModels/             ← DATA SHAPED FOR SCREENS (never raw DB entities).
│   ├── LoginViewModel.cs, RegisterViewModel.cs
│   ├── SupplierProfileEditViewModel.cs, ProductCreateEditViewModel.cs
│   ├── StoreProfileEditViewModel.cs, AddInventoryItemViewModel.cs, UpdateStockViewModel.cs
│   ├── OrderCreateViewModel.cs, AddOrderItemViewModel.cs
│   ├── ShipmentCreateViewModel.cs, UpdateShipmentStatusViewModel.cs
│   └── InboxViewModel.cs        Includes ConversationSummary + ComposeMessageViewModel.
│
├── Views/                  ← THE HTML PAGES (Razor .cshtml = HTML + C#).
│   ├── Shared/
│   │   ├── _Layout.cshtml       Master template: navbar, sidebar, footer. Every page
│   │   │                         plugs into this so the whole site looks consistent.
│   │   └── _ValidationScriptsPartial.cshtml   Client-side form validation scripts.
│   ├── _ViewImports.cshtml      Shared "using" statements + Tag Helper registration.
│   ├── _ViewStart.cshtml        Says "every view uses _Layout by default."
│   ├── Home/    (Index dashboard, Privacy)
│   ├── Account/ (Login, Register, AccessDenied)
│   ├── Suppliers/ (Index, Details, EditProfile)
│   ├── Products/  (Index, Create, Edit, Details, Delete)
│   ├── Stores/    (Index, Create, Details, Inventory, AddProduct, UpdateStock, EditProfile)
│   ├── Orders/    (Index, Create, AddItem, Details)
│   ├── Shipments/ (Index, Create, Details, UpdateStatus)
│   └── Messages/  (Inbox, Thread, Compose)
│
├── Migrations/             ← AUTO-GENERATED database change history.
│   ├── 20260517065050_InitialCreate.cs     First schema (all tables).
│   ├── 20260517091641_HQStoreModel.cs      Change: HQ-managed store model.
│   └── AppDbContextModelSnapshot.cs         EF's current picture of the whole schema.
│
└── wwwroot/                ← STATIC PUBLIC FILES served as-is.
    ├── css/site.css            Custom styles on top of Bootstrap.
    ├── js/site.js              Custom JavaScript.
    └── lib/                    Bootstrap & jQuery (bundled by the template).
```

**Why this structure?** It follows the **Separation of Concerns** principle: each folder has
exactly one responsibility. When something breaks, you know *where* to look — a wrong price
is in Services, a broken button is in Views, a missing column is in Models/Migrations.

---

## 7. The architecture — the 4 layers

The app is organized in **four layers**, and data flows through them in order. This is the
single most important idea to understand.

```
   ┌─────────────────────────────────────────────────────────────┐
   │  LAYER 1 — PRESENTATION   (Views + ViewModels)                │
   │  The HTML pages the user sees. Dumb on purpose: only displays.│
   └───────────────────────────┬─────────────────────────────────┘
                               │  ViewModels carry data up/down
   ┌───────────────────────────▼─────────────────────────────────┐
   │  LAYER 2 — CONTROLLERS                                        │
   │  Receive the request, decide what to do, pick a View.         │
   │  Stay THIN: no business logic, no SQL. Just coordinate.       │
   └───────────────────────────┬─────────────────────────────────┘
                               │  call service methods
   ┌───────────────────────────▼─────────────────────────────────┐
   │  LAYER 3 — SERVICES  (business logic)                         │
   │  The rules: "an order total = sum of items", "mark as read",  │
   │  "log every status change". The brain of the app.             │
   └───────────────────────────┬─────────────────────────────────┘
                               │  query via EF Core
   ┌───────────────────────────▼─────────────────────────────────┐
   │  LAYER 4 — DATA  (AppDbContext + Models + PostgreSQL)         │
   │  Stores and retrieves everything. EF Core turns C# ↔ SQL.     │
   └─────────────────────────────────────────────────────────────┘
```

**Why bother with layers?**

- **Testable** — you can test business rules (Services) without a browser.
- **Swappable** — you could replace PostgreSQL or the UI without rewriting the rules.
- **Understandable** — three teammates who don't know C# can each learn *one* layer.
- **Safe** — controllers can't accidentally write raw SQL; the database can't leak into the UI.

### How DI (Dependency Injection) wires the layers together

In `Program.cs` we register each service:

```csharp
builder.Services.AddScoped<IOrderService, OrderService>();
```

This says: *"Whenever any class asks for an `IOrderService`, hand it a fresh `OrderService`
for the duration of this request."* So `OrdersController` simply declares in its constructor
that it *needs* an `IOrderService`, and .NET **injects** one automatically. The controller
never writes `new OrderService(...)`. This is DI — the framework builds and supplies your
dependencies, keeping classes loosely coupled and easy to change.

`AddScoped` = **one instance per HTTP request**. This matters because the `DbContext` is also
scoped, so everything in a single request shares the same database connection and transaction.

---

## 8. The database — tables and relationships

Every class in `Models/` becomes a table. EF Core reads these classes and (via migrations)
creates matching PostgreSQL tables. Relationships are configured in
`Data/AppDbContext.cs → OnModelCreating()` using the **Fluent API**.

### Entity Relationship map

```
ApplicationUser (a login account; has a Role: Supplier | StoreManager | Driver)
   ├──1:1──▶ Supplier   (if role = Supplier)
   └──1:1──▶ Driver     (if role = Driver)
            (StoreManager = HQ; manages ALL stores, not tied to one)

Supplier ──1:many──▶ Product        (a supplier's catalog)
Supplier ──1:many──▶ Order          (orders it receives)

Store    ──1:many──▶ InventoryItem  (its current stock)
Store    ──1:many──▶ Order          (orders it places)

Product  ──1:many──▶ InventoryItem  (stocked in many stores)
Product  ──1:many──▶ OrderItem      (appears in many order lines)

Order    ──1:many──▶ OrderItem      (the products + quantities in it)
Order    ──1:1────▶ Shipment        (created once confirmed)

Driver   ──1:many──▶ Shipment       (deliveries over time)

Shipment ──1:many──▶ DeliveryStatusLog  (append-only status history)

Message  ──▶ Sender (User), Receiver (User)
Message  ──▶ optionally Order and/or Shipment  (context link)
```

### The two status "state machines"

**Order status** (a business document's lifecycle):

```
Draft ──▶ Submitted ──▶ Confirmed ──▶ Fulfilled
   └──────────┴────────────┴──▶ Cancelled
```
Each transition is timestamped on the `Order`: `CreatedAt`, `SubmittedAt`, `ConfirmedAt`,
`FulfilledAt`.

**Shipment status** (the physical delivery):

```
Pending ──▶ Dispatched ──▶ InTransit ──▶ Delivered
                                    └──▶ Failed
```
Every change appends a **new row** to `DeliveryStatusLog` (with a timestamp and *which user*
made the change) — the old status is never erased. That's the audit-log pattern.

### `OnDelete` behaviors — what happens when a parent is deleted

The Fluent API also defines deletion rules, which protect your data:

| Behavior | Meaning | Example in this app |
|---|---|---|
| **Cascade** | Delete the children too. | Delete an Order → its `OrderItem`s and `Shipment` go with it. Delete a Store → its inventory goes. |
| **Restrict** | Block the delete if children exist. | Can't delete a Supplier that still has Products/Orders — prevents accidental data loss. |
| **SetNull** | Keep the child, just clear the link. | Delete a Driver → their Shipments stay but `DriverId` becomes null. Delete a linked Order → a Message's `OrderId` becomes null. |

### A note on money precision

Money fields (`Product.UnitPrice`, `Order.TotalAmount`, `OrderItem.UnitPrice`) are declared as
C# `decimal` and configured with `.HasPrecision(18, 2)` — up to 18 digits with 2 decimal
places. We use `decimal` (not `float`/`double`) because floating-point math rounds badly and
you must **never** round money incorrectly.

---

## 9. Authentication & roles — who can do what

Authentication is handled by **ASP.NET Identity**, which gives us secure registration, login,
logout, password **hashing** (passwords are never stored as plain text), and **roles** — all
for free. After login, the user's browser holds a **cookie** (a small signed token). Every
later request includes that cookie, so the server knows who you are without asking you to log
in again. The cookie slides to a 7-day expiry with activity.

There are **three roles**, and each sees a different dashboard and has different powers:

| Role | Who they are | Dashboard shows | Can do |
|---|---|---|---|
| **Supplier** | A company selling goods | Incoming orders, shipment statuses, unread messages | Manage their products, confirm/reject orders, create shipments, message stores |
| **StoreManager** (HQ) | The head office running all branches | Inventory alerts, order history, delivery tracking | Manage stores & inventory, place orders, track deliveries, message suppliers |
| **Driver** | A delivery person | Assigned shipments, delivery schedule | View their shipments, update delivery status, message store managers |

Roles are enforced with the **`[Authorize]`** attribute on controllers. For example:

```csharp
[Authorize(Roles = "Supplier,StoreManager,Driver")]   // all three may use the inbox
public class MessagesController : Controller { ... }
```

If a logged-out user hits a protected page, they're redirected to `/Account/Login`. If a
logged-in user hits a page their role can't access, they're sent to `/Account/AccessDenied`.
Both paths are configured in `Program.cs`.

Password rules are intentionally relaxed for a university demo (min 6 chars, one digit) and
accounts lock for 5 minutes after 5 failed attempts.

---

## 10. The modules — feature by feature

The app was built in **phases**, each a self-contained module. Here's what each does.

### Phase 1 — Setup & Foundation
The skeleton: all models, the `AppDbContext`, Identity with 3 roles, cookie login/register,
the Bootstrap sidebar layout, the PostgreSQL connection, and `SeedData`. Everything else
builds on this.

### Phase 2 — Supplier & Product Management
- **`SuppliersController`** — a supplier edits their own company profile; store managers can
  browse supplier catalogs (read-only).
- **`ProductsController`** — full CRUD (Create/Read/Update/Delete) for a supplier's products.
- A supplier can only edit **their own** products; a store manager can only **view**.

### Phase 3 — Store & Inventory Management
- **`StoresController`** — HQ manages all branch stores (create, edit, view).
- **Inventory tracking** — each `InventoryItem` records how many of a product a store holds.
- **Low-stock alerts** — items below their threshold are highlighted (red) so HQ knows to
  reorder. This is the model the IDE currently has open nearby.

### Phase 4 — Order Management (the core feature)
- **`OrdersController`** — the full order lifecycle.
- A store manager **creates** an order, picks a supplier, and **adds items** (each becomes an
  `OrderItem` with product, quantity, and a captured unit price). The order's `TotalAmount`
  is computed from its items — this logic lives in `OrderService`.
- The supplier **confirms or rejects** it, moving the status along the state machine.
- An order **Details** page shows every line, the total, and the status timeline.

### Phase 5 — Shipment & Delivery Tracking
- **`ShipmentsController`** — a supplier creates a `Shipment` from a confirmed order and
  assigns a `Driver`.
- Every status change appends a **`DeliveryStatusLog`** entry (append-only history), recording
  the new status, timestamp, and which user made the change.
- The driver sees their assigned shipments and updates status (e.g. mark Delivered).
- **Estimated vs actual** delivery dates are tracked, and a **timeline view** shows the full
  chronological history — like a courier tracking page.

### Phase 6 — Inbox / Messaging (the currently active branch)
- **`MessagesController`** + **`MessageService`** — a lightweight email/chat system.
- **Inbox** (`/Messages/Inbox`): the service pulls every message you sent or received, then
  **groups them by conversation partner** so you see one row per person, with the latest
  message preview and an **unread count**.
- **Thread** (`/Messages/Thread?partnerId=…`): the full chat with one person, oldest-to-newest.
  **Opening a thread marks the partner's messages to you as read** (that's how the unread badge
  clears), and this is done inside `GetThreadAsync` by flipping `IsRead` and stamping `ReadAt`.
- **Compose** (`/Messages/Compose`): start a new message; can be **pre-filled** from elsewhere
  (e.g. a "Message supplier about this order" button links to `?to=…&orderId=…`), which is how
  messages get **context-linked** to an order or shipment.
- **Reply**: the box at the bottom of a thread posts a quick reply back into the same conversation.
- Notice the clean split: the controller is thin; grouping, unread-counting, and read-marking
  all live in `MessageService`.

### Phase 7 — Dashboards & Polish *(planned)*
Role-specific dashboards with cards, counts, quick-action buttons, breadcrumbs, and toast
notifications — to make the app presentation-ready.

### Phase 8 — Deployment *(planned)*
Dockerfile, `render.yaml`, environment-based config, run migrations on startup, a `/health`
endpoint, and a polished README.

---

## 11. Complete end-to-end story (a full user journey)

Let's follow **one carton of USB-C cables** through the entire system, touching every module.

1. **A supplier lists a product.**
   *Ahmed Khan (Khan Electronics, role = Supplier)* logs in. The cookie remembers him.
   He opens **Products → Create**, adds "USB-C Cable (2m)" at Rs. 350. `ProductsController`
   calls `SupplierService`, EF Core inserts a row into the `Products` table.

2. **HQ notices low stock.**
   *The StoreManager (HQ)* opens **Stores → Inventory** for the Karachi branch. The USB-C
   cable stock is below its threshold and shows in **red**. Time to reorder.

3. **HQ places an order.**
   HQ opens **Orders → Create**, selects *Khan Electronics* as the supplier, then **Add Item**:
   50× USB-C Cable. `OrderService` creates an `Order` (status **Draft**) and one `OrderItem`,
   and computes `TotalAmount = 50 × 350`. HQ hits **Submit** → status becomes **Submitted**,
   `SubmittedAt` is stamped.

4. **The supplier confirms.**
   Ahmed sees the incoming order on his dashboard, opens its **Details**, and clicks **Confirm**.
   Status → **Confirmed**, `ConfirmedAt` stamped.

5. **The supplier ships it.**
   Ahmed opens **Shipments → Create** from the confirmed order, assigns *the Driver*, sets a
   tracking number and estimated delivery date. A `Shipment` is created (status **Pending**),
   and the **first** `DeliveryStatusLog` row is written.

6. **The driver delivers.**
   *The Driver* logs in, sees the shipment on their list, and updates status as they go:
   **Dispatched → InTransit → Delivered**. Each tap appends a **new** `DeliveryStatusLog` row
   (with time + driver id). On **Delivered**, the shipment's `ActualDeliveryDate` is set and the
   order can be marked **Fulfilled** (`FulfilledAt` stamped).

7. **They coordinate via the inbox.**
   Midway, HQ opens the order and clicks "Message supplier about this order," which opens
   **Compose** pre-filled with the supplier as recipient and `OrderId` attached. They exchange
   messages in a **Thread**; the unread badge appears for the recipient and clears when they
   open it.

8. **Everything is auditable.**
   Anyone viewing the shipment sees the **full timeline** — exactly when it was dispatched, went
   in transit, and delivered, and *who* did each step. Nothing was overwritten; every step is on
   the record.

That single journey exercised **all four architecture layers** and **six of the eight modules**.

---

## 12. How the app starts up (Program.cs line by line)

`Program.cs` is the very first code that runs. It has two halves.

### Part 1 — Register services (the "builder" section)

```csharp
var builder = WebApplication.CreateBuilder(args);
```
Creates the app builder — a collector for all the configuration and services the app needs.

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
```
Registers the database: *"Use PostgreSQL (via Npgsql), and get the connection string from
config."* Now any class can ask for an `AppDbContext`.

```csharp
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => { ...password/lockout rules... })
    .AddEntityFrameworkStores<AppDbContext>()   // store users in our PostgreSQL DB
    .AddDefaultTokenProviders();
```
Turns on the whole Identity system — login, hashing, roles — and tells it to store users in
our database.

```csharp
builder.Services.ConfigureApplicationCookie(options => {
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
});
```
Configures the login cookie: where to redirect unauthenticated/unauthorized users and how long
the session lasts.

```csharp
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IStoreService, StoreService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IShipmentService, ShipmentService>();
// (MessageService is registered the same way on the messaging branch)
```
Registers each business-logic service for **Dependency Injection**, one instance per request.

```csharp
builder.Services.AddControllersWithViews();
```
Turns on **MVC** — controllers + Razor views.

### Part 2 — Build the app & the request pipeline (the "app" section)

```csharp
var app = builder.Build();
```
Assembles everything into a runnable app.

```csharp
using (var scope = app.Services.CreateScope())
{
    var context     = services.GetRequiredService<AppDbContext>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    await SeedData.InitializeAsync(context, userManager, roleManager);
}
```
**Before** accepting any traffic, it seeds the database: runs migrations, creates the 3 roles,
test users, suppliers, products, branches, inventory, and demo orders — but only if they don't
already exist. It's wrapped in a temporary DI **scope** because seeding happens outside a normal
request, and in a `try/catch` so a seeding hiccup logs an error instead of crashing the app.

Then the **middleware pipeline** is set up — the ordered checkpoints every request passes:

```csharp
app.UseHttpsRedirection();   // force HTTPS
app.UseStaticFiles();        // serve wwwroot (CSS/JS/images)
app.UseRouting();            // figure out which controller/action matches the URL
app.UseAuthentication();     // "Who is this user?" (read the cookie)
app.UseAuthorization();      // "Are they allowed here?" (check roles)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();
```

**Order matters.** Authentication must come before Authorization (you must know *who* someone is
before checking *what* they're allowed to do). The route pattern
`{controller=Home}/{action=Index}/{id?}` means the URL `/Orders/Details/5` maps to
`OrdersController.Details(id: 5)`, and a bare `/` defaults to `HomeController.Index`.
`app.Run()` starts listening for requests.

---

## 13. Running, building, and deploying

### Everyday commands (run from `src/SupplyChainMS/`)

```bash
dotnet run              # start the app once
dotnet watch run        # start with hot reload (auto-restarts on code change)
dotnet build            # compile and check for errors WITHOUT running
```

### Managing the database (EF Core migrations)

```bash
dotnet ef migrations add <Name>   # record a schema change into Migrations/
dotnet ef database update         # apply pending migrations to the DB
```
A **migration** is a versioned recipe describing how to change the database's shape (add a
table, add a column…). The app also runs migrations automatically on startup (inside
`SeedData.InitializeAsync` via `context.Database.MigrateAsync()`), so the DB is always
up to date when it launches.

### Adding a library

```bash
dotnet add package <PackageName>   # installs a NuGet package (like pip install)
```

### Configuration & secrets

- `appsettings.json` — safe, public settings (logging).
- `appsettings.Development.json` — **secret** local settings, including the Supabase connection
  string with the DB password. This file is in `.gitignore` and **must never be committed**.
- In production (Render.com), the connection string comes from an **environment variable**
  (`ConnectionStrings__DefaultConnection`) set in the dashboard — never hard-coded.

### Deployment (Render.com + Docker) — planned for Phase 8

- A multi-stage **Dockerfile** builds the app and produces a small runnable image.
- **`render.yaml`** tells Render to run it on the free tier and where to read env vars.
- Push to GitHub → Render auto-builds the Docker image and hosts it at a public URL.

### Git branching model

`main` is the single integration/production branch. **Never push directly to `main`.** Each
phase is built on its own branch (`phase-3/store`, `phase-6/inbox`, …) and merged into `main`
via a Pull Request when complete. The messaging module is currently on the active feature branch.

---

## 14. Glossary — quick reference of every term

| Term | One-line definition |
|---|---|
| **.NET 8** | The runtime/platform that executes C# code. |
| **ASP.NET Core** | The web framework part of .NET. |
| **C#** | The programming language (Java-like). |
| **MVC** | Model–View–Controller: separates data, HTML, and request-handling. |
| **Razor / .cshtml** | Template files mixing HTML with C# to render dynamic pages. |
| **Controller** | Class that handles web requests for one topic. |
| **Action** | One method in a controller answering one URL. |
| **Model / Entity** | A C# class mapped to one database table. |
| **Service** | Class holding business logic between controllers and the DB. |
| **Interface** | A contract (`IOrderService`) listing what a service can do. |
| **ViewModel** | A class shaped for one screen, not a raw table. |
| **EF Core** | The ORM mapping C# classes ↔ SQL tables. |
| **ORM** | Object-Relational Mapper — no raw SQL needed. |
| **Npgsql** | The PostgreSQL driver for .NET/EF Core. |
| **DbContext** | The object representing the whole database connection. |
| **DbSet<T>** | One table exposed as a queryable collection. |
| **Fluent API** | Configuring relationships via chained method calls. |
| **Migration** | A versioned, recorded change to the DB structure. |
| **Dependency Injection (DI)** | Framework supplies the objects a class needs. |
| **Scoped** | A DI lifetime: one instance per HTTP request. |
| **Middleware** | Ordered steps every request passes through. |
| **Tag Helper** | HTML attributes (`asp-for`) that C# fills in. |
| **async / await** | Non-blocking code so the app stays responsive. |
| **ASP.NET Identity** | Built-in system for users, passwords, and roles. |
| **Cookie authentication** | A signed browser token that remembers a logged-in user. |
| **`[Authorize]`** | Attribute restricting a page to logged-in users / roles. |
| **`TempData`** | A one-shot message that survives a redirect. |
| **`wwwroot`** | Public folder for static CSS/JS/image files. |
| **NuGet** | .NET's package manager. |
| **Supabase** | The free hosted PostgreSQL database provider. |
| **Bootstrap 5** | The CSS framework for layout and components. |
| **Docker** | Packages the app so it runs identically anywhere. |
| **Render.com** | The free hosting service that runs the deployed app. |
| **Audit log / event log** | Append-only history table (e.g. `DeliveryStatusLog`). |
| **State machine** | A defined set of statuses and allowed transitions. |

---

*This document describes the SupplyChainMS project as built through Phase 6 (messaging).
Dashboards (Phase 7) and deployment (Phase 8) are designed but not yet implemented.*
