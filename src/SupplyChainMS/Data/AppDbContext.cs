// ============================================================
// AppDbContext.cs — The "gateway" to the database
// ============================================================
//
// DbContext is EF Core's central class. It:
//   1. Knows about all our tables (via DbSet<T> properties)
//   2. Manages the connection to PostgreSQL
//   3. Translates C# queries into SQL
//   4. Tracks changes and saves them with SaveChangesAsync()
//
// The IdentityDbContext<ApplicationUser> base class is a special version
// that ALSO includes all the Identity tables:
//   - AspNetUsers (our ApplicationUser)
//   - AspNetRoles
//   - AspNetUserRoles
//   - AspNetUserClaims
//   - etc.
//
// We just need to extend it with OUR tables.

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SupplyChainMS.Models;

namespace SupplyChainMS.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    // The constructor receives "options" from Program.cs (like the connection string).
    // We pass them up to the base class — it handles everything.
    // This is "dependency injection" for the database configuration.
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // -------------------------------------------------------
    // DbSet<T> — Each one of these = one table in the database.
    // Think of DbSet like a Python list that syncs with the DB.
    //
    // Usage example:
    //   var suppliers = await _context.Suppliers.ToListAsync();
    //   var supplier = await _context.Suppliers.FindAsync(id);
    //   _context.Suppliers.Add(newSupplier);
    //   await _context.SaveChangesAsync();
    // -------------------------------------------------------

    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Store> Stores { get; set; }
    public DbSet<InventoryItem> InventoryItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Shipment> Shipments { get; set; }
    public DbSet<DeliveryStatusLog> DeliveryStatusLogs { get; set; }
    public DbSet<Driver> Drivers { get; set; }
    public DbSet<Message> Messages { get; set; }

    // -------------------------------------------------------
    // OnModelCreating — Configure all table relationships
    // -------------------------------------------------------
    //
    // "Fluent API" = configuring relationships using method chains.
    // This is an alternative to putting [ForeignKey] attributes on models.
    // We prefer this because it keeps model files clean and groups all
    // DB configuration in ONE place.
    //
    // Pattern:
    //   entity.HasOne(...)     — "this entity has ONE of that"
    //   entity.HasMany(...)    — "this entity has MANY of those"
    //   .WithOne(...)          — "the other side has ONE back"
    //   .WithMany(...)         — "the other side has MANY back"
    //   .HasForeignKey(...)    — "use this property as the FK column"
    //   .OnDelete(...)         — what happens when the parent is deleted?
    //
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // IMPORTANT: Always call base first — it sets up Identity tables
        base.OnModelCreating(modelBuilder);

        // -------------------------------------------------------
        // ApplicationUser → Supplier (one-to-one)
        // One user CAN HAVE one supplier profile.
        // If the user is deleted, cascade-delete their supplier profile too.
        // -------------------------------------------------------
        modelBuilder.Entity<ApplicationUser>()
            .HasOne(u => u.SupplierProfile)
            .WithOne(s => s.User)
            .HasForeignKey<Supplier>(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // -------------------------------------------------------
        // ApplicationUser → Store (one-to-one)
        // One user manages one store.
        // -------------------------------------------------------
        modelBuilder.Entity<ApplicationUser>()
            .HasOne(u => u.ManagedStore)
            .WithOne(s => s.Manager)
            .HasForeignKey<Store>(s => s.ManagerUserId)
            .OnDelete(DeleteBehavior.Cascade);

        // -------------------------------------------------------
        // ApplicationUser → Driver (one-to-one)
        // -------------------------------------------------------
        modelBuilder.Entity<ApplicationUser>()
            .HasOne(u => u.DriverProfile)
            .WithOne(d => d.User)
            .HasForeignKey<Driver>(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // -------------------------------------------------------
        // Supplier → Products (one-to-many)
        // One supplier has MANY products.
        // If supplier is deleted → restrict (don't auto-delete products,
        // show an error instead so data isn't accidentally lost)
        // -------------------------------------------------------
        modelBuilder.Entity<Supplier>()
            .HasMany(s => s.Products)
            .WithOne(p => p.Supplier)
            .HasForeignKey(p => p.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        // -------------------------------------------------------
        // Supplier → Orders (one-to-many)
        // -------------------------------------------------------
        modelBuilder.Entity<Supplier>()
            .HasMany(s => s.Orders)
            .WithOne(o => o.Supplier)
            .HasForeignKey(o => o.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        // -------------------------------------------------------
        // Store → InventoryItems (one-to-many)
        // If a store is deleted, delete all its inventory records too
        // -------------------------------------------------------
        modelBuilder.Entity<Store>()
            .HasMany(s => s.InventoryItems)
            .WithOne(i => i.Store)
            .HasForeignKey(i => i.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        // -------------------------------------------------------
        // Store → Orders (one-to-many)
        // -------------------------------------------------------
        modelBuilder.Entity<Store>()
            .HasMany(s => s.Orders)
            .WithOne(o => o.Store)
            .HasForeignKey(o => o.StoreId)
            .OnDelete(DeleteBehavior.Restrict);

        // -------------------------------------------------------
        // Product → InventoryItems (one-to-many)
        // -------------------------------------------------------
        modelBuilder.Entity<Product>()
            .HasMany(p => p.InventoryItems)
            .WithOne(i => i.Product)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // -------------------------------------------------------
        // Product → OrderItems (one-to-many)
        // -------------------------------------------------------
        modelBuilder.Entity<Product>()
            .HasMany(p => p.OrderItems)
            .WithOne(oi => oi.Product)
            .HasForeignKey(oi => oi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // -------------------------------------------------------
        // Order → OrderItems (one-to-many)
        // If the order is deleted, cascade-delete its line items
        // -------------------------------------------------------
        modelBuilder.Entity<Order>()
            .HasMany(o => o.Items)
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // -------------------------------------------------------
        // Order → Shipment (one-to-one)
        // If the order is deleted, delete the shipment too
        // -------------------------------------------------------
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Shipment)
            .WithOne(s => s.Order)
            .HasForeignKey<Shipment>(s => s.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // -------------------------------------------------------
        // Driver → Shipments (one-to-many)
        // One driver can have many shipments over time.
        // SetNull: if driver is deleted, shipment stays but DriverId becomes null
        // -------------------------------------------------------
        modelBuilder.Entity<Driver>()
            .HasMany(d => d.Shipments)
            .WithOne(s => s.Driver)
            .HasForeignKey(s => s.DriverId)
            .OnDelete(DeleteBehavior.SetNull);

        // -------------------------------------------------------
        // Shipment → DeliveryStatusLogs (one-to-many)
        // -------------------------------------------------------
        modelBuilder.Entity<Shipment>()
            .HasMany(s => s.StatusLogs)
            .WithOne(l => l.Shipment)
            .HasForeignKey(l => l.ShipmentId)
            .OnDelete(DeleteBehavior.Cascade);

        // -------------------------------------------------------
        // DeliveryStatusLog → ApplicationUser (many-to-one)
        // The user who updated the status.
        // Restrict so we can't accidentally delete a user who has logs.
        // -------------------------------------------------------
        modelBuilder.Entity<DeliveryStatusLog>()
            .HasOne(l => l.UpdatedByUser)
            .WithMany()
            .HasForeignKey(l => l.UpdatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // -------------------------------------------------------
        // Message → Sender and Receiver
        // A message has TWO foreign keys to ApplicationUser.
        // We must name them explicitly to avoid EF Core confusion.
        // -------------------------------------------------------
        modelBuilder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Receiver)
            .WithMany()
            .HasForeignKey(m => m.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);

        // -------------------------------------------------------
        // Message → Order (optional link)
        // -------------------------------------------------------
        modelBuilder.Entity<Message>()
            .HasOne(m => m.Order)
            .WithMany()
            .HasForeignKey(m => m.OrderId)
            .OnDelete(DeleteBehavior.SetNull);

        // -------------------------------------------------------
        // Message → Shipment (optional link)
        // -------------------------------------------------------
        modelBuilder.Entity<Message>()
            .HasOne(m => m.Shipment)
            .WithMany()
            .HasForeignKey(m => m.ShipmentId)
            .OnDelete(DeleteBehavior.SetNull);

        // -------------------------------------------------------
        // Decimal precision — PostgreSQL needs explicit precision for money fields.
        // (18, 2) means: up to 18 digits total, 2 decimal places. e.g., 9999999999999999.99
        // -------------------------------------------------------
        modelBuilder.Entity<Product>()
            .Property(p => p.UnitPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Order>()
            .Property(o => o.TotalAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<OrderItem>()
            .Property(oi => oi.UnitPrice)
            .HasPrecision(18, 2);
    }
}
