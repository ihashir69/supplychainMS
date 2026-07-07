# 📦 SupplyChainMS — Supply Chain Management System

A web-based supply chain management system built with **ASP.NET Core 8 MVC**
and **PostgreSQL**. It models a company (think a restaurant chain like KFC)
running multiple branches that order stock from suppliers, with drivers
delivering shipments — plus a built-in messaging inbox tying it all together.

> University project @ University of Karachi. Built with ASP.NET Core MVC,
> Entity Framework Core, and Bootstrap 5.

---

## ✨ Features

| Module | What it does |
|--------|--------------|
| **Authentication** | Register/login with 3 roles (Supplier, StoreManager/HQ, Driver) |
| **Suppliers & Products** | Suppliers manage product catalogs; HQ browses them |
| **Stores & Inventory** | Per-branch inventory with low-stock alerts |
| **Orders** | HQ places orders → supplier confirms → lifecycle tracking |
| **Shipments** | Create shipments, assign drivers, append-only status history |
| **Inbox / Messaging** | Any two users can chat; messages link to orders/shipments |
| **Dashboards** | Role-specific live stats, quick actions, recent activity |

---

## 🛠️ Tech Stack

- **Backend + UI:** ASP.NET Core 8 MVC + Razor Views
- **Language:** C# (.NET 8)
- **ORM:** Entity Framework Core + Npgsql
- **Database:** PostgreSQL (Supabase free tier)
- **CSS:** Bootstrap 5 + Bootstrap Icons
- **Auth:** ASP.NET Identity + cookie authentication
- **Hosting:** Render.com (Docker)

---

## 🚀 Running Locally

### 1. Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- A PostgreSQL database. Easiest option: a free
  [Supabase](https://supabase.com) project. (Local Postgres also works.)

### 2. Configure the database connection

The app reads its connection string from
`src/SupplyChainMS/appsettings.Development.json`.
This file is **git-ignored** because it holds your DB password — create it
yourself with this content:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=db.XXXXX.supabase.co;Database=postgres;Username=postgres;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true"
  }
}
```

Replace `XXXXX` and `YOUR_PASSWORD` with your Supabase project's values
(Supabase dashboard → Project Settings → Database → Connection string).

### 3. Run it

```bash
cd src/SupplyChainMS
dotnet run
```

Then open the URL shown in the terminal (e.g. `https://localhost:5001`).

> **Migrations & seed data run automatically on startup.** The first time the
> app boots against an empty database, it creates all the tables and fills
> them with demo data (suppliers, branches, products, orders). You don't need
> to run any migration commands by hand.

For auto-reload while editing code:

```bash
dotnet watch run
```

---

## 🧪 Test Login Credentials

The seed data creates these accounts (all use the password `Test@123`):

| Role | Email | Password |
|------|-------|----------|
| Supplier | `supplier@test.com` | `Test@123` |
| Store Manager (HQ) | `storemanager@test.com` | `Test@123` |
| Driver | `driver@test.com` | `Test@123` |

---

## 🌐 Deploying to Render.com

The repo is deployment-ready via `Dockerfile` + `render.yaml` at the root.

1. Push the code to GitHub.
2. Go to [render.com](https://render.com) → **New** → **Web Service** →
   connect your GitHub repo. Render auto-detects `render.yaml`.
3. In the service's **Environment** settings, add the secret:
   - **Key:** `ConnectionStrings__DefaultConnection`
   - **Value:** your Supabase connection string (same format as above)
4. Click **Deploy**. Render builds the Docker image and starts the app.
5. Render pings `/health` to confirm the deploy is alive.

`ASPNETCORE_ENVIRONMENT` is already set to `Production` by `render.yaml`.

---

## 📁 Project Structure

```
SupplyChainMS/
├── Dockerfile            ← Container recipe for deployment
├── render.yaml           ← Render.com hosting config
├── .dockerignore         ← Files kept out of the Docker build
├── CLAUDE.md             ← Full project spec & build plan
└── src/SupplyChainMS/
    ├── Program.cs        ← App startup (services + middleware)
    ├── Models/           ← Database entities
    ├── Data/             ← DbContext + seed data
    ├── Services/         ← Business logic (one per module)
    ├── Controllers/      ← Handle HTTP requests
    ├── ViewModels/       ← Data shaped for the UI
    ├── Views/            ← Razor pages (HTML + C#)
    └── Migrations/       ← EF Core schema history
```

---

## 👥 Team

3 members — presentation split by module (see `CLAUDE.md` for assignments).
All code authored with Claude Code, heavily commented for a team new to C#/.NET.

---

## 🔐 Security Note

Never commit `appsettings.Development.json` — it contains your database
password and is git-ignored on purpose. In production the connection string
is provided as an environment variable on Render, never stored in the repo.
