// ============================================================
// UpdateStockViewModel.cs — Form data for adjusting stock quantity
// ============================================================
//
// Used when a StoreManager manually updates how many units of a
// product they currently have in stock (e.g. after a delivery arrives
// or after a physical stock count).

using System.ComponentModel.DataAnnotations;

namespace SupplyChainMS.ViewModels;

public class UpdateStockViewModel
{
    // Which inventory record to update
    public int InventoryItemId { get; set; }

    // For display purposes on the form (not editable)
    public string ProductName { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative")]
    [Display(Name = "Current Quantity in Stock")]
    public int QuantityInStock { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Threshold must be at least 1")]
    [Display(Name = "Low Stock Alert Threshold")]
    public int LowStockThreshold { get; set; }
}
