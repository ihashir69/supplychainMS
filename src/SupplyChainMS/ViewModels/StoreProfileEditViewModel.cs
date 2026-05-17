// ============================================================
// StoreProfileEditViewModel.cs — Form data for editing a store profile
// ============================================================

using System.ComponentModel.DataAnnotations;

namespace SupplyChainMS.ViewModels;

public class StoreProfileEditViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Store name is required")]
    [StringLength(200)]
    [Display(Name = "Store Name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Address is required")]
    [StringLength(500)]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Contact phone is required")]
    [StringLength(20)]
    [Display(Name = "Contact Phone")]
    public string ContactPhone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Contact email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [Display(Name = "Contact Email")]
    public string ContactEmail { get; set; } = string.Empty;
}
