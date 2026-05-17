// ============================================================
// Supplier.cs — Represents a company that sells products
// ============================================================
//
// This is a "Model" — it maps directly to a database table called "Suppliers".
// Each property (public string Name) becomes a column in that table.
//
// In Python/SQLModel terms:
//   class Supplier(SQLModel, table=True):
//       name: str
//       ...
//
// The "Id" property is the Primary Key (unique identifier for each row).

namespace SupplyChainMS.Models;

public class Supplier
{
    // Primary key — EF Core automatically makes a property called "Id"
    // the primary key and auto-increments it. (Like SERIAL in SQL)
    public int Id { get; set; }

    // The foreign key — which ApplicationUser "owns" this supplier profile.
    // This links back to the AspNetUsers table created by Identity.
    public string UserId { get; set; } = string.Empty;

    public string CompanyName { get; set; } = string.Empty;

    public string ContactEmail { get; set; } = string.Empty;

    public string ContactPhone { get; set; } = string.Empty;

    // Where is this supplier located?
    public string Address { get; set; } = string.Empty;

    // A short description of what they sell
    public string? Description { get; set; }

    // Is this supplier active? (We "soft delete" by setting this to false
    // instead of actually deleting the row — preserves order history)
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // -------------------------------------------------------
    // Navigation Properties
    // -------------------------------------------------------

    // The user account that owns this supplier profile
    public ApplicationUser User { get; set; } = null!;

    // One supplier can have MANY products (one-to-many relationship)
    // ICollection = a list of items
    public ICollection<Product> Products { get; set; } = new List<Product>();

    // One supplier can receive MANY orders from stores
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
