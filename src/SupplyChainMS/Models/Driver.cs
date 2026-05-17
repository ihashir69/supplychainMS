// ============================================================
// Driver.cs — A delivery driver profile
// ============================================================
//
// Like Supplier, a Driver is a USER with a specific profile.
// The ApplicationUser account handles login; this table stores
// driver-specific info like their vehicle details.

namespace SupplyChainMS.Models;

public class Driver
{
    public int Id { get; set; }

    // Foreign Key → links back to the login account
    public string UserId { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    // Vehicle plate number for tracking purposes
    public string VehiclePlate { get; set; } = string.Empty;

    // Type of vehicle (e.g., "Van", "Truck", "Motorcycle")
    public string VehicleType { get; set; } = string.Empty;

    // Is the driver currently available for new assignments?
    public bool IsAvailable { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // -------------------------------------------------------
    // Navigation Properties
    // -------------------------------------------------------

    public ApplicationUser User { get; set; } = null!;

    // A driver can be assigned to MANY shipments over time
    public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
}
