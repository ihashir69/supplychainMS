// ============================================================
// ApplicationUser.cs — The "User" table in our database
// ============================================================
//
// ASP.NET Identity already gives us a built-in user system
// (like Django's auth.User). It handles:
//   - Password hashing
//   - Login/logout
//   - Session cookies
//
// But we need to ADD a "Role" field (Supplier, StoreManager, Driver).
// To do that, we EXTEND the built-in IdentityUser class.
//
// In Java terms: this is like extending an abstract class.
// In Python terms: this is like subclassing Django's AbstractUser.
//
// The : IdentityUser syntax means "inherit from IdentityUser".
// IdentityUser already has: Id, Email, UserName, PasswordHash, etc.
// We are just adding our custom fields on top.

using Microsoft.AspNetCore.Identity;

namespace SupplyChainMS.Models;

public class ApplicationUser : IdentityUser
{
    // The role this user plays in the system.
    // We store it as a string ("Supplier", "StoreManager", "Driver")
    // so it's human-readable in the database.
    public string Role { get; set; } = string.Empty;

    // The user's display name (separate from their login username)
    public string FullName { get; set; } = string.Empty;

    // When did they create their account?
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // -------------------------------------------------------
    // Navigation Properties — these are NOT database columns.
    // EF Core uses them to understand relationships between tables.
    // Think of them as "foreign key shortcuts" that let you write:
    //   user.SupplierProfile.CompanyName
    // instead of doing a manual JOIN.
    // -------------------------------------------------------

    // If this user is a Supplier, they have one supplier profile.
    // "?" means nullable — StoreManagers and Drivers won't have this.
    public Supplier? SupplierProfile { get; set; }

    // StoreManager role = HQ managing ALL stores — no single store assigned here.

    // If this user is a Driver, they have one driver profile.
    public Driver? DriverProfile { get; set; }
}
