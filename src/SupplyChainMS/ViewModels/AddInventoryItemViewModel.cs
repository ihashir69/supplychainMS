// ============================================================
// AddInventoryItemViewModel.cs — Form data for adding a product to inventory
// ============================================================
//
// When a StoreManager wants to track a new product in their store,
// they pick from a dropdown of available products, set an initial
// quantity, and set the low stock threshold (the alert level).

using System.ComponentModel.DataAnnotations;

namespace SupplyChainMS.ViewModels;

public class AddInventoryItemViewModel
{
    // Which store is adding this product (set by controller, not user)
    public int StoreId { get; set; }

    [Required(ErrorMessage = "Please select a product")]
    [Display(Name = "Product")]
    public int ProductId { get; set; }

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative")]
    [Display(Name = "Initial Quantity in Stock")]
    public int QuantityInStock { get; set; } = 0;

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Threshold must be at least 1")]
    [Display(Name = "Low Stock Alert Threshold")]
    // When stock drops to or below this number, show a warning
    public int LowStockThreshold { get; set; } = 10;
}
