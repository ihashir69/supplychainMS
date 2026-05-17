// ============================================================
// RegisterViewModel.cs — Data the registration form collects
// ============================================================

using System.ComponentModel.DataAnnotations;

namespace SupplyChainMS.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Full name is required")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    // [Compare] checks that this field matches the Password field.
    // Catches typos when setting a new password.
    [Required(ErrorMessage = "Please confirm your password")]
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    // Which role are they registering as?
    [Required(ErrorMessage = "Please select a role")]
    public string Role { get; set; } = string.Empty;
}
