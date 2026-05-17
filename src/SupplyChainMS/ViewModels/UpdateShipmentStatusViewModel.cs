// ============================================================
// UpdateShipmentStatusViewModel.cs — Form for updating delivery status
// ============================================================
//
// Used by the Driver to advance the shipment status.
// They pick the new status and optionally add a note
// (e.g. "Arrived at warehouse", "Delivered to branch manager Sara Ali").

using System.ComponentModel.DataAnnotations;
using SupplyChainMS.Models;

namespace SupplyChainMS.ViewModels;

public class UpdateShipmentStatusViewModel
{
    public int ShipmentId { get; set; }

    // For display on the form
    public string TrackingNumber { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public ShipmentStatus CurrentStatus { get; set; }

    [Required(ErrorMessage = "Please select a new status")]
    [Display(Name = "New Status")]
    public ShipmentStatus NewStatus { get; set; }

    [StringLength(500)]
    [Display(Name = "Notes (optional)")]
    public string? Notes { get; set; }
}
