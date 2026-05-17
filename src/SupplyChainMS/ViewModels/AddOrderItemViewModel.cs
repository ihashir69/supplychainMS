// ============================================================
// AddOrderItemViewModel.cs — Form data for adding a product to an order
// ============================================================

using System.ComponentModel.DataAnnotations;

namespace SupplyChainMS.ViewModels;

public class AddOrderItemViewModel
{
    public int OrderId { get; set; }
    public int SupplierId { get; set; }

    [Required(ErrorMessage = "Please select a product")]
    [Display(Name = "Product")]
    public int ProductId { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; } = 1;
}
