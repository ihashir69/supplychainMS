// ============================================================
// SupplierProfileEditViewModel.cs — Data shape for editing a supplier profile
// ============================================================
//
// A "ViewModel" is a class shaped specifically for one view (one page).
// We NEVER pass raw database entities (like Supplier.cs) to views because:
//   1. The entity might have fields the user shouldn't see or edit
//   2. The entity might not have everything the view needs
//   3. Model binding (reading form data) is safer with a ViewModel
//
// This ViewModel is used by:
//   - GET  /Suppliers/EditProfile  → pre-fill the form with current data
//   - POST /Suppliers/EditProfile  → read back the submitted form data
//
// DataAnnotations (like [Required]) tell ASP.NET:
//   - What validation rules to apply
//   - What error messages to show in the form
//   - These run BEFORE the controller action body even starts

using System.ComponentModel.DataAnnotations;

namespace SupplyChainMS.ViewModels;

public class SupplierProfileEditViewModel
{
    // Hidden field — we pass this through the form so we know WHICH supplier to update.
    // The user never sees or types this.
    public int Id { get; set; }

    [Required(ErrorMessage = "Company name is required")]
    [StringLength(200, ErrorMessage = "Company name cannot exceed 200 characters")]
    [Display(Name = "Company Name")]
    public string CompanyName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Contact email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [Display(Name = "Contact Email")]
    public string ContactEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Contact phone is required")]
    [Display(Name = "Contact Phone")]
    [StringLength(20)]
    public string ContactPhone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Address is required")]
    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    public string Address { get; set; } = string.Empty;

    // Optional — no [Required] attribute
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    [Display(Name = "About Your Business")]
    public string? Description { get; set; }
}
