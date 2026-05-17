// ============================================================
// SeedData.cs — Populate the database with test data on startup
// ============================================================
//
// "Seeding" = inserting initial data into a fresh database.
// This runs ONCE when the app starts if the DB is empty.
//
// We create:
//   - 3 Roles: Supplier, StoreManager, Driver
//   - 1 test user per role (with preset passwords from CLAUDE.md)
//   - Profile records for each user
//   - Sample products
//
// This is like Django's fixtures or SQLModel's test data scripts.
//
// This class is "static" — meaning we don't create an object of it.
// We just call SeedData.InitializeAsync(...) directly.

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SupplyChainMS.Models;

namespace SupplyChainMS.Data;

public static class SeedData
{
    // The 3 roles in our system — these map to [Authorize(Roles = "...")] checks
    public const string SupplierRole = "Supplier";
    public const string StoreManagerRole = "StoreManager";
    public const string DriverRole = "Driver";

    // This is an "async" method — it uses "await" to do DB operations
    // without blocking the server while waiting for the DB to respond.
    // Think of it like: instead of waiting in line, you get a ticket and
    // come back when it's your turn. The server can handle other requests meanwhile.
    public static async Task InitializeAsync(
        AppDbContext context,
        UserManager<ApplicationUser> userManager,      // handles user creation with password hashing
        RoleManager<IdentityRole> roleManager)          // handles role creation
    {
        // Apply any pending database migrations automatically.
        // Like running "python manage.py migrate" in Django — it creates
        // all the tables if they don't exist yet.
        await context.Database.MigrateAsync();

        // -------------------------------------------------------
        // Step 1: Create Roles (if they don't already exist)
        // -------------------------------------------------------
        // We check first with roleManager.RoleExistsAsync to avoid
        // creating duplicates if the app restarts.
        string[] roles = [SupplierRole, StoreManagerRole, DriverRole];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // -------------------------------------------------------
        // Step 2: Create test Supplier user
        // -------------------------------------------------------
        const string supplierEmail = "supplier@test.com";
        const string supplierPassword = "Test@123";

        // Only create if this user doesn't exist yet
        if (await userManager.FindByEmailAsync(supplierEmail) == null)
        {
            var supplierUser = new ApplicationUser
            {
                UserName = supplierEmail,
                Email = supplierEmail,
                FullName = "Ahmed Khan",
                Role = SupplierRole,
                EmailConfirmed = true    // Skip email verification for dev
            };

            // CreateAsync hashes the password automatically — never store plain text!
            var result = await userManager.CreateAsync(supplierUser, supplierPassword);
            if (result.Succeeded)
            {
                // Assign the "Supplier" role to this user
                await userManager.AddToRoleAsync(supplierUser, SupplierRole);

                // Create the supplier profile linked to this user
                var supplier = new Supplier
                {
                    UserId = supplierUser.Id,
                    CompanyName = "Khan Electronics",
                    ContactEmail = supplierEmail,
                    ContactPhone = "0300-1234567",
                    Address = "Plot 45, SITE Industrial Area, Karachi",
                    Description = "Leading supplier of electronic components and accessories",
                    IsActive = true
                };
                context.Suppliers.Add(supplier);
                await context.SaveChangesAsync();

                // Add some sample products for this supplier
                var products = new List<Product>
                {
                    new Product
                    {
                        SupplierId = supplier.Id,
                        Name = "USB-C Cable (2m)",
                        Description = "High-speed USB-C data and charging cable",
                        UnitPrice = 450.00m,   // the 'm' suffix = decimal literal in C#
                        Unit = "piece",
                        Category = "Electronics",
                        StockQuantity = 500
                    },
                    new Product
                    {
                        SupplierId = supplier.Id,
                        Name = "HDMI Cable (1.5m)",
                        Description = "4K HDMI cable with gold-plated connectors",
                        UnitPrice = 750.00m,
                        Unit = "piece",
                        Category = "Electronics",
                        StockQuantity = 300
                    },
                    new Product
                    {
                        SupplierId = supplier.Id,
                        Name = "Wireless Mouse",
                        Description = "Ergonomic wireless mouse with USB receiver",
                        UnitPrice = 1200.00m,
                        Unit = "piece",
                        Category = "Electronics",
                        StockQuantity = 150
                    }
                };
                context.Products.AddRange(products);
                await context.SaveChangesAsync();
            }
        }

        // -------------------------------------------------------
        // Step 3: Create test StoreManager user
        // -------------------------------------------------------
        const string storeManagerEmail = "storemanager@test.com";
        const string storeManagerPassword = "Test@123";

        if (await userManager.FindByEmailAsync(storeManagerEmail) == null)
        {
            var storeManagerUser = new ApplicationUser
            {
                UserName = storeManagerEmail,
                Email = storeManagerEmail,
                FullName = "Sara Ali",
                Role = StoreManagerRole,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(storeManagerUser, storeManagerPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(storeManagerUser, StoreManagerRole);

                // Create the store profile
                var store = new Store
                {
                    ManagerUserId = storeManagerUser.Id,
                    Name = "City Centre Electronics",
                    Address = "Shop 12, City Centre Mall, Shahrah-e-Faisal, Karachi",
                    ContactPhone = "021-34521890",
                    ContactEmail = storeManagerEmail,
                    IsActive = true
                };
                context.Stores.Add(store);
                await context.SaveChangesAsync();
            }
        }

        // -------------------------------------------------------
        // Step 4: Create test Driver user
        // -------------------------------------------------------
        const string driverEmail = "driver@test.com";
        const string driverPassword = "Test@123";

        if (await userManager.FindByEmailAsync(driverEmail) == null)
        {
            var driverUser = new ApplicationUser
            {
                UserName = driverEmail,
                Email = driverEmail,
                FullName = "Bilal Raza",
                Role = DriverRole,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(driverUser, driverPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(driverUser, DriverRole);

                // Create driver profile
                var driver = new Driver
                {
                    UserId = driverUser.Id,
                    FullName = "Bilal Raza",
                    PhoneNumber = "0333-9876543",
                    VehiclePlate = "KHI-4521",
                    VehicleType = "Delivery Van",
                    IsAvailable = true
                };
                context.Drivers.Add(driver);
                await context.SaveChangesAsync();
            }
        }
    }
}
