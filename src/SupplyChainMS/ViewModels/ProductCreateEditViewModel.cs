// ============================================================
// ProductCreateEditViewModel.cs — Data shape for creating/editing a product
// ============================================================
//
// Used for both the Create and Edit product forms.
// Having one ViewModel for both saves code repetition — the forms
// look almost identical, just the heading and button text differ.
//
// The controller checks: if Id == 0, it's a Create. If Id > 0, it's an Edit.

using System.ComponentModel.DataAnnotations;

namespace SupplyChainMS.ViewModels;

public class ProductCreateEditViewModel
{
    // 0 for new products, >0 for existing ones being edited
    public int Id { get; set; }

    // Hidden — which supplier owns this product (set by controller, not user input)
    public int SupplierId { get; set; }

    [Required(ErrorMessage = "Product name is required")]
    [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
    [Display(Name = "Product Name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    [Display(Name = "Description (optional)")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Price is required")]
    [Range(0.01, 9999999, ErrorMessage = "Price must be greater than 0")]
    [Display(Name = "Unit Price (PKR)")]
    // DataType.Currency = hint to the browser to show currency formatting
    [DataType(DataType.Currency)]
    public decimal UnitPrice { get; set; }

    [Required(ErrorMessage = "Unit is required")]
    [StringLength(50)]
    [Display(Name = "Sold In (e.g. piece, kg, box)")]
    public string Unit { get; set; } = "piece";

    [StringLength(100)]
    [Display(Name = "Category (e.g. Electronics, Food)")]
    public string? Category { get; set; }

    [Required(ErrorMessage = "Stock quantity is required")]
    [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative")]
    [Display(Name = "Stock Quantity")]
    public int StockQuantity { get; set; }
}
