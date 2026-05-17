// ============================================================
// SeedData.cs — Full demo data for SupplyChainMS
// ============================================================
//
// Simulates a company (like KFC) running 10 city branches,
// ordering from 3 external suppliers.
//
// What gets created:
//   Roles:     Supplier, StoreManager, Driver
//   Users:     1 HQ manager, 3 suppliers, 1 driver
//   Suppliers: Khan Electronics, Pak Office Supplies, TechZone Accessories
//   Products:  17 products across 3 suppliers
//   Branches:  10 across Karachi, Lahore, Islamabad, RWP, FSD, Peshawar
//   Inventory: 4-6 products stocked per branch (mix of healthy/low/out)
//   Orders:    10 orders showing all lifecycle stages
//              (Draft, Submitted, Confirmed, Fulfilled, Cancelled)

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SupplyChainMS.Models;

namespace SupplyChainMS.Data;

public static class SeedData
{
    public const string SupplierRole    = "Supplier";
    public const string StoreManagerRole = "StoreManager";
    public const string DriverRole      = "Driver";

    public static async Task InitializeAsync(
        AppDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        await context.Database.MigrateAsync();

        // -------------------------------------------------------
        // ROLES
        // -------------------------------------------------------
        foreach (var role in new[] { SupplierRole, StoreManagerRole, DriverRole })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // -------------------------------------------------------
        // SUPPLIER 1 — Khan Electronics
        // login: supplier@test.com / Test@123
        // -------------------------------------------------------
        Supplier? khanElectronics = await GetOrCreateSupplier(context, userManager,
            email: "supplier@test.com",
            fullName: "Ahmed Khan",
            company: "Khan Electronics",
            phone: "0300-1234567",
            address: "Plot 45, SITE Industrial Area, Karachi",
            description: "Leading supplier of electronic components, cables, and peripherals since 2005.",
            products: s => new List<Product>
            {
                new() { SupplierId = s.Id, Name = "USB-C Cable (2m)",
                    Description = "High-speed USB-C cable, 5A rated, braided nylon",
                    UnitPrice = 450m, Unit = "piece", Category = "Cables", StockQuantity = 800 },
                new() { SupplierId = s.Id, Name = "HDMI Cable (1.5m)",
                    Description = "4K HDMI 2.0 cable with gold-plated connectors",
                    UnitPrice = 750m, Unit = "piece", Category = "Cables", StockQuantity = 500 },
                new() { SupplierId = s.Id, Name = "Wireless Mouse",
                    Description = "Ergonomic 2.4GHz wireless mouse with USB receiver",
                    UnitPrice = 1200m, Unit = "piece", Category = "Peripherals", StockQuantity = 300 },
                new() { SupplierId = s.Id, Name = "Mechanical Keyboard",
                    Description = "TKL mechanical keyboard with blue switches",
                    UnitPrice = 3500m, Unit = "piece", Category = "Peripherals", StockQuantity = 150 },
                new() { SupplierId = s.Id, Name = "USB Hub (7-port)",
                    Description = "Powered 7-port USB 3.0 hub with individual on/off switches",
                    UnitPrice = 1800m, Unit = "piece", Category = "Accessories", StockQuantity = 400 },
                new() { SupplierId = s.Id, Name = "Laptop Stand",
                    Description = "Adjustable aluminum stand, fits 10–17 inch laptops",
                    UnitPrice = 2200m, Unit = "piece", Category = "Accessories", StockQuantity = 200 },
            });

        // -------------------------------------------------------
        // SUPPLIER 2 — Pak Office Supplies
        // login: supplier2@test.com / Test@123
        // -------------------------------------------------------
        Supplier? pakOffice = await GetOrCreateSupplier(context, userManager,
            email: "supplier2@test.com",
            fullName: "Fatima Malik",
            company: "Pak Office Supplies",
            phone: "0321-9876543",
            address: "Shop 7, Tariq Road Commercial Area, Karachi",
            description: "Stationery, office furniture, and consumables. Serving businesses since 2010.",
            products: s => new List<Product>
            {
                new() { SupplierId = s.Id, Name = "A4 Paper Ream (500 sheets)",
                    Description = "80gsm white A4 printing paper, acid-free",
                    UnitPrice = 850m, Unit = "ream", Category = "Stationery", StockQuantity = 2000 },
                new() { SupplierId = s.Id, Name = "Ballpoint Pens (box of 50)",
                    Description = "Blue ink medium-tip ballpoint pens",
                    UnitPrice = 400m, Unit = "box", Category = "Stationery", StockQuantity = 1000 },
                new() { SupplierId = s.Id, Name = "Whiteboard Markers (set of 4)",
                    Description = "Dry-erase markers — black, blue, red, green",
                    UnitPrice = 350m, Unit = "set", Category = "Stationery", StockQuantity = 600 },
                new() { SupplierId = s.Id, Name = "Office Chair",
                    Description = "Ergonomic mesh chair with lumbar support and adjustable armrests",
                    UnitPrice = 18500m, Unit = "piece", Category = "Furniture", StockQuantity = 50 },
                new() { SupplierId = s.Id, Name = "Printer Ink Cartridge (Black)",
                    Description = "Compatible with HP, Canon, and Epson models",
                    UnitPrice = 1200m, Unit = "piece", Category = "Consumables", StockQuantity = 400 },
            });

        // -------------------------------------------------------
        // SUPPLIER 3 — TechZone Accessories
        // login: supplier3@test.com / Test@123
        // -------------------------------------------------------
        Supplier? techZone = await GetOrCreateSupplier(context, userManager,
            email: "supplier3@test.com",
            fullName: "Omar Siddiqui",
            company: "TechZone Accessories",
            phone: "0333-5512378",
            address: "Unit 12, Korangi Industrial Area, Karachi",
            description: "Premium tech accessories — monitors, headsets, webcams, and networking gear.",
            products: s => new List<Product>
            {
                new() { SupplierId = s.Id, Name = "24-inch Monitor (FHD)",
                    Description = "1080p IPS display, 75Hz, HDMI + VGA ports",
                    UnitPrice = 32000m, Unit = "piece", Category = "Displays", StockQuantity = 60 },
                new() { SupplierId = s.Id, Name = "USB Headset with Mic",
                    Description = "Stereo headset with noise-cancelling microphone, USB plug",
                    UnitPrice = 2800m, Unit = "piece", Category = "Audio", StockQuantity = 200 },
                new() { SupplierId = s.Id, Name = "HD Webcam (1080p)",
                    Description = "Auto-focus webcam with built-in microphone, USB-A",
                    UnitPrice = 4500m, Unit = "piece", Category = "Video", StockQuantity = 120 },
                new() { SupplierId = s.Id, Name = "Network Switch (8-port)",
                    Description = "Unmanaged Gigabit switch, plug-and-play",
                    UnitPrice = 5500m, Unit = "piece", Category = "Networking", StockQuantity = 80 },
                new() { SupplierId = s.Id, Name = "Surge Protector (6-outlet)",
                    Description = "6-outlet power strip with surge protection and 2 USB charging ports",
                    UnitPrice = 1500m, Unit = "piece", Category = "Power", StockQuantity = 350 },
                new() { SupplierId = s.Id, Name = "Cable Management Kit",
                    Description = "Velcro straps, cable clips, and sleeves for desk organization",
                    UnitPrice = 600m, Unit = "set", Category = "Accessories", StockQuantity = 500 },
            });

        // -------------------------------------------------------
        // STORE MANAGER — HQ
        // login: storemanager@test.com / Test@123
        // -------------------------------------------------------
        if (await userManager.FindByEmailAsync("storemanager@test.com") == null)
        {
            var u = new ApplicationUser { UserName = "storemanager@test.com", Email = "storemanager@test.com", FullName = "Sara Ali", Role = StoreManagerRole, EmailConfirmed = true };
            var r = await userManager.CreateAsync(u, "Test@123");
            if (r.Succeeded) await userManager.AddToRoleAsync(u, StoreManagerRole);
        }

        // -------------------------------------------------------
        // DRIVER
        // login: driver@test.com / Test@123
        // -------------------------------------------------------
        if (await userManager.FindByEmailAsync("driver@test.com") == null)
        {
            var u = new ApplicationUser { UserName = "driver@test.com", Email = "driver@test.com", FullName = "Bilal Raza", Role = DriverRole, EmailConfirmed = true };
            var r = await userManager.CreateAsync(u, "Test@123");
            if (r.Succeeded)
            {
                await userManager.AddToRoleAsync(u, DriverRole);
                context.Drivers.Add(new Driver { UserId = u.Id, FullName = "Bilal Raza", PhoneNumber = "0333-9876543", VehiclePlate = "KHI-4521", VehicleType = "Delivery Van", IsAvailable = true });
                await context.SaveChangesAsync();
            }
        }

        // -------------------------------------------------------
        // 10 BRANCHES — all managed by HQ, no per-store ownership
        // If we have fewer than 10 branches, wipe and re-seed everything
        // so old single-store seed data doesn't block the demo.
        // -------------------------------------------------------
        var storeCount = await context.Stores.CountAsync();
        if (storeCount < 10)
        {
            // Clear dependent data first (FK order matters)
            context.OrderItems.RemoveRange(context.OrderItems);
            context.Orders.RemoveRange(context.Orders);
            context.InventoryItems.RemoveRange(context.InventoryItems);
            context.Stores.RemoveRange(context.Stores);
            await context.SaveChangesAsync();
        }

        if (!await context.Stores.AnyAsync())
        {
            context.Stores.AddRange(
                // Index 0
                new Store { Name = "Karachi Main Branch",        Address = "Shop 12, City Centre Mall, Shahrah-e-Faisal, Karachi",  ContactPhone = "021-34521890", ContactEmail = "karachi.main@company.com",    IsActive = true },
                // Index 1
                new Store { Name = "Karachi Clifton Branch",     Address = "Plot 8, Clifton Block 5, Karachi",                       ContactPhone = "021-35831201", ContactEmail = "karachi.clifton@company.com", IsActive = true },
                // Index 2
                new Store { Name = "Karachi Gulshan Branch",     Address = "Shop 4, Gulshan-e-Iqbal Block 13, Karachi",             ContactPhone = "021-34902311", ContactEmail = "karachi.gulshan@company.com", IsActive = true },
                // Index 3
                new Store { Name = "Lahore Defence Branch",      Address = "Plot 5, DHA Phase 4, Lahore",                           ContactPhone = "042-35678901", ContactEmail = "lahore.dha@company.com",      IsActive = true },
                // Index 4
                new Store { Name = "Lahore Gulberg Branch",      Address = "45-B, Main Gulberg, Lahore",                            ContactPhone = "042-35761422", ContactEmail = "lahore.gulberg@company.com",  IsActive = true },
                // Index 5
                new Store { Name = "Islamabad F-10 Branch",      Address = "Shop 3, F-10 Markaz, Islamabad",                        ContactPhone = "051-22345678", ContactEmail = "islamabad.f10@company.com",   IsActive = true },
                // Index 6
                new Store { Name = "Islamabad Blue Area Branch", Address = "2nd Floor, Jinnah Avenue, Blue Area, Islamabad",         ContactPhone = "051-22819034", ContactEmail = "islamabad.blue@company.com",  IsActive = true },
                // Index 7
                new Store { Name = "Rawalpindi Saddar Branch",   Address = "Shop 7, Saddar Bazaar, Rawalpindi",                      ContactPhone = "051-35512987", ContactEmail = "rwp.saddar@company.com",      IsActive = true },
                // Index 8
                new Store { Name = "Faisalabad D-Ground Branch", Address = "Opp. D-Ground, Jhang Road, Faisalabad",                 ContactPhone = "041-26710234", ContactEmail = "fsd.dground@company.com",     IsActive = true },
                // Index 9
                new Store { Name = "Peshawar University Branch", Address = "Shop 2, University Road, Peshawar",                     ContactPhone = "091-92801234", ContactEmail = "psh.uni@company.com",         IsActive = true }
            );
            await context.SaveChangesAsync();
        }

        // -------------------------------------------------------
        // INVENTORY — 4-6 products per branch
        // Mix of: healthy stock / low stock / out of stock
        // -------------------------------------------------------
        // Re-seed inventory if it's empty (happens after store wipe above)
        if (!await context.InventoryItems.AnyAsync())
        {
            var stores   = await context.Stores.OrderBy(s => s.Id).ToListAsync();
            var products = await context.Products.ToDictionaryAsync(p => p.Name);
            var items    = new List<InventoryItem>();

            void Stock(int storeIdx, string name, int qty, int threshold)
            {
                if (storeIdx < stores.Count && products.TryGetValue(name, out var p))
                    items.Add(new InventoryItem { StoreId = stores[storeIdx].Id, ProductId = p.Id, QuantityInStock = qty, LowStockThreshold = threshold, LastUpdated = DateTime.UtcNow });
            }

            // 0 Karachi Main
            Stock(0, "USB-C Cable (2m)",            45, 20);
            Stock(0, "HDMI Cable (1.5m)",            30, 15);
            Stock(0, "Wireless Mouse",               20, 10);
            Stock(0, "Mechanical Keyboard",           8, 10);  // LOW
            Stock(0, "A4 Paper Ream (500 sheets)",   60, 20);
            Stock(0, "Printer Ink Cartridge (Black)", 5, 10);  // LOW
            Stock(0, "Surge Protector (6-outlet)",   25, 10);

            // 1 Karachi Clifton
            Stock(1, "USB-C Cable (2m)",              0, 20);  // OUT
            Stock(1, "Wireless Mouse",               40, 10);
            Stock(1, "USB Hub (7-port)",              22, 10);
            Stock(1, "A4 Paper Ream (500 sheets)",   80, 20);
            Stock(1, "Ballpoint Pens (box of 50)",   12, 10);  // LOW
            Stock(1, "USB Headset with Mic",         18, 10);

            // 2 Karachi Gulshan
            Stock(2, "HDMI Cable (1.5m)",            15, 15);
            Stock(2, "Laptop Stand",                  6,  5);
            Stock(2, "Office Chair",                  3,  5);  // LOW
            Stock(2, "Whiteboard Markers (set of 4)", 30, 10);
            Stock(2, "HD Webcam (1080p)",              9, 5);
            Stock(2, "Cable Management Kit",          50, 15);

            // 3 Lahore Defence
            Stock(3, "USB-C Cable (2m)",             12, 20);  // LOW
            Stock(3, "Wireless Mouse",               35, 10);
            Stock(3, "USB Hub (7-port)",              18, 10);
            Stock(3, "A4 Paper Ream (500 sheets)",    0, 20);  // OUT
            Stock(3, "Whiteboard Markers (set of 4)", 25, 10);
            Stock(3, "Office Chair",                   4,  5);

            // 4 Lahore Gulberg
            Stock(4, "Mechanical Keyboard",          50, 10);
            Stock(4, "Laptop Stand",                 20,  5);
            Stock(4, "USB Hub (7-port)",               3, 10);  // LOW
            Stock(4, "A4 Paper Ream (500 sheets)",   40, 20);
            Stock(4, "Printer Ink Cartridge (Black)", 0, 10);   // OUT
            Stock(4, "Network Switch (8-port)",       7,  5);
            Stock(4, "24-inch Monitor (FHD)",          2,  3);  // LOW

            // 5 Islamabad F-10
            Stock(5, "USB-C Cable (2m)",             80, 20);
            Stock(5, "HDMI Cable (1.5m)",            50, 15);
            Stock(5, "Mechanical Keyboard",          22, 10);
            Stock(5, "A4 Paper Ream (500 sheets)",   40, 20);
            Stock(5, "Ballpoint Pens (box of 50)",   30, 10);
            Stock(5, "24-inch Monitor (FHD)",          5,  3);

            // 6 Islamabad Blue Area
            Stock(6, "Wireless Mouse",               10, 10);  // AT threshold
            Stock(6, "USB Hub (7-port)",               8, 10);  // LOW
            Stock(6, "Office Chair",                  12,  5);
            Stock(6, "Printer Ink Cartridge (Black)", 20, 10);
            Stock(6, "HD Webcam (1080p)",              0,  5);  // OUT
            Stock(6, "Surge Protector (6-outlet)",    40, 10);

            // 7 Rawalpindi Saddar
            Stock(7, "USB-C Cable (2m)",            100, 20);
            Stock(7, "HDMI Cable (1.5m)",            60, 15);
            Stock(7, "Ballpoint Pens (box of 50)",    5, 10);  // LOW
            Stock(7, "A4 Paper Ream (500 sheets)",   25, 20);
            Stock(7, "USB Headset with Mic",         30, 10);
            Stock(7, "Cable Management Kit",          70, 20);

            // 8 Faisalabad
            Stock(8, "Wireless Mouse",                0, 10);  // OUT
            Stock(8, "Mechanical Keyboard",           14, 10);
            Stock(8, "Laptop Stand",                   9,  5);
            Stock(8, "Whiteboard Markers (set of 4)", 40, 10);
            Stock(8, "Network Switch (8-port)",        2,  3);  // LOW
            Stock(8, "Surge Protector (6-outlet)",    15, 10);

            // 9 Peshawar
            Stock(9, "USB-C Cable (2m)",              30, 20);
            Stock(9, "A4 Paper Ream (500 sheets)",     7, 20);  // LOW
            Stock(9, "Ballpoint Pens (box of 50)",     0, 10);  // OUT
            Stock(9, "Office Chair",                    6,  5);
            Stock(9, "USB Headset with Mic",           12, 10);
            Stock(9, "Cable Management Kit",           45, 15);

            context.InventoryItems.AddRange(items);
            await context.SaveChangesAsync();
        }

        // -------------------------------------------------------
        // ORDERS — 10 orders showing every lifecycle status
        // Draft, Submitted, Confirmed, Fulfilled, Cancelled
        // -------------------------------------------------------
        if (!await context.Orders.AnyAsync() && khanElectronics != null && pakOffice != null && techZone != null)
        {
            var stores   = await context.Stores.OrderBy(s => s.Id).ToListAsync();
            var products = await context.Products.ToDictionaryAsync(p => p.Name);

            if (stores.Count >= 10)
            {
                // Helper to create an order + items in one call
                async Task<Order> MakeOrder(Store store, Supplier supplier, OrderStatus status,
                    string? notes, DateTime created, DateTime? submitted, DateTime? confirmed, DateTime? fulfilled,
                    params (string productName, int qty)[] lines)
                {
                    var order = new Order
                    {
                        StoreId = store.Id, SupplierId = supplier.Id, Status = status, Notes = notes,
                        CreatedAt = created, SubmittedAt = submitted, ConfirmedAt = confirmed, FulfilledAt = fulfilled
                    };
                    context.Orders.Add(order);
                    await context.SaveChangesAsync();

                    var orderItems = new List<OrderItem>();
                    foreach (var (name, qty) in lines)
                        if (products.TryGetValue(name, out var p))
                            orderItems.Add(new OrderItem { OrderId = order.Id, ProductId = p.Id, Quantity = qty, UnitPrice = p.UnitPrice });

                    context.OrderItems.AddRange(orderItems);
                    order.TotalAmount = orderItems.Sum(i => i.Quantity * i.UnitPrice);
                    await context.SaveChangesAsync();
                    return order;
                }

                var now = DateTime.UtcNow;

                // ORDER 1 — Fulfilled: Karachi Main restocked cables (complete cycle)
                await MakeOrder(stores[0], khanElectronics, OrderStatus.Fulfilled,
                    "Monthly cable restock",
                    now.AddDays(-12), now.AddDays(-11), now.AddDays(-10), now.AddDays(-7),
                    ("USB-C Cable (2m)", 100), ("HDMI Cable (1.5m)", 50), ("Mechanical Keyboard", 20));

                // ORDER 2 — Confirmed: Lahore Defence (awaiting shipment creation)
                await MakeOrder(stores[3], khanElectronics, OrderStatus.Confirmed,
                    "Priority — USB cables out of stock",
                    now.AddDays(-5), now.AddDays(-4), now.AddDays(-3), null,
                    ("USB-C Cable (2m)", 200), ("USB Hub (7-port)", 50), ("Laptop Stand", 25));

                // ORDER 3 — Submitted: Lahore Defence office supplies (waiting for Pak Office to confirm)
                await MakeOrder(stores[3], pakOffice, OrderStatus.Submitted,
                    "Urgent — paper completely out",
                    now.AddDays(-2), now.AddDays(-1), null, null,
                    ("A4 Paper Ream (500 sheets)", 50), ("Ballpoint Pens (box of 50)", 10), ("Printer Ink Cartridge (Black)", 20));

                // ORDER 4 — Draft: Islamabad F-10 building order (not submitted yet)
                await MakeOrder(stores[5], khanElectronics, OrderStatus.Draft,
                    "Q3 restocking",
                    now.AddHours(-3), null, null, null,
                    ("USB Hub (7-port)", 30), ("Wireless Mouse", 15));

                // ORDER 5 — Fulfilled: Islamabad Blue Area furniture (complete)
                await MakeOrder(stores[6], pakOffice, OrderStatus.Fulfilled,
                    "New office setup",
                    now.AddDays(-20), now.AddDays(-19), now.AddDays(-18), now.AddDays(-15),
                    ("Office Chair", 8), ("Whiteboard Markers (set of 4)", 20), ("A4 Paper Ream (500 sheets)", 30));

                // ORDER 6 — Cancelled: Rawalpindi order cancelled after submission
                await MakeOrder(stores[7], pakOffice, OrderStatus.Cancelled,
                    "Budget freeze — cancelled",
                    now.AddDays(-8), now.AddDays(-7), null, null,
                    ("Office Chair", 5), ("A4 Paper Ream (500 sheets)", 20));

                // ORDER 7 — Confirmed: Karachi Clifton tech upgrade (awaiting shipment)
                await MakeOrder(stores[1], techZone, OrderStatus.Confirmed,
                    "Branch upgrade — new monitors and headsets",
                    now.AddDays(-4), now.AddDays(-3), now.AddDays(-2), null,
                    ("24-inch Monitor (FHD)", 5), ("USB Headset with Mic", 10), ("HD Webcam (1080p)", 5));

                // ORDER 8 — Submitted: Faisalabad to TechZone (waiting for confirmation)
                await MakeOrder(stores[8], techZone, OrderStatus.Submitted,
                    "Networking equipment needed",
                    now.AddHours(-18), now.AddHours(-12), null, null,
                    ("Network Switch (8-port)", 3), ("Surge Protector (6-outlet)", 15), ("Cable Management Kit", 20));

                // ORDER 9 — Draft: Peshawar building a new order
                await MakeOrder(stores[9], pakOffice, OrderStatus.Draft,
                    null,
                    now.AddHours(-1), null, null, null,
                    ("A4 Paper Ream (500 sheets)", 40), ("Ballpoint Pens (box of 50)", 20));

                // ORDER 10 — Fulfilled: Karachi Gulshan (complete, older order)
                await MakeOrder(stores[2], techZone, OrderStatus.Fulfilled,
                    "Initial stock for new branch",
                    now.AddDays(-30), now.AddDays(-29), now.AddDays(-28), now.AddDays(-25),
                    ("HD Webcam (1080p)", 6), ("USB Headset with Mic", 12), ("Surge Protector (6-outlet)", 20), ("Cable Management Kit", 30));
            }
        }
    }

    // -------------------------------------------------------
    // Helper: create a supplier + products, or fetch if exists
    // -------------------------------------------------------
    private static async Task<Supplier?> GetOrCreateSupplier(
        AppDbContext context,
        UserManager<ApplicationUser> userManager,
        string email, string fullName, string company,
        string phone, string address, string description,
        Func<Supplier, List<Product>> products)
    {
        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser != null)
            return await context.Suppliers.FirstOrDefaultAsync(s => s.UserId == existingUser.Id);

        var user = new ApplicationUser
        {
            UserName = email, Email = email,
            FullName = fullName, Role = SupplierRole, EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(user, "Test@123");
        if (!result.Succeeded) return null;

        await userManager.AddToRoleAsync(user, SupplierRole);

        var supplier = new Supplier
        {
            UserId = user.Id, CompanyName = company, ContactEmail = email,
            ContactPhone = phone, Address = address, Description = description, IsActive = true
        };
        context.Suppliers.Add(supplier);
        await context.SaveChangesAsync();

        context.Products.AddRange(products(supplier));
        await context.SaveChangesAsync();

        return supplier;
    }
}
