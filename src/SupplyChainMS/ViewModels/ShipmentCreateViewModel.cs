// ============================================================
// ShipmentCreateViewModel.cs — Form for creating a shipment
// ============================================================
//
// Used by the Supplier after confirming an order.
// They pick a driver, set an estimated delivery date, and add notes.

using System.ComponentModel.DataAnnotations;

namespace SupplyChainMS.ViewModels;

public class ShipmentCreateViewModel
{
    // Which confirmed order this shipment is for (hidden, set by controller)
    public int OrderId { get; set; }

    // For display on the form (not submitted)
    public string StoreName { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;

    [Display(Name = "Assign Driver")]
    public int? DriverId { get; set; }   // Optional — can dispatch without driver assigned

    [Display(Name = "Estimated Delivery Date")]
    [DataType(DataType.Date)]
    public DateTime? EstimatedDeliveryDate { get; set; }

    [StringLength(500)]
    [Display(Name = "Delivery Notes (optional)")]
    public string? DeliveryNotes { get; set; }
}
