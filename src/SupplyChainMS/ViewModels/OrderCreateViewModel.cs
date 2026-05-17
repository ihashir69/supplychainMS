// ============================================================
// OrderCreateViewModel.cs — Form data for starting a new order
// ============================================================
//
// HQ (StoreManager) picks which branch is ordering and which supplier.
// After submission a Draft order is created and they add products next.

using System.ComponentModel.DataAnnotations;

namespace SupplyChainMS.ViewModels;

public class OrderCreateViewModel
{
    [Required(ErrorMessage = "Please select a branch")]
    [Display(Name = "Branch / Store")]
    public int StoreId { get; set; }

    [Required(ErrorMessage = "Please select a supplier")]
    [Display(Name = "Supplier")]
    public int SupplierId { get; set; }

    [StringLength(500)]
    [Display(Name = "Notes (optional)")]
    public string? Notes { get; set; }
}
