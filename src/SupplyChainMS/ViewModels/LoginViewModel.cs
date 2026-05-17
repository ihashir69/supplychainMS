// ============================================================
// LoginViewModel.cs — Data the login form collects
// ============================================================
//
// A "ViewModel" is NOT a database model. It's just a container
// for the data we need to display a specific view (page).
//
// Why not use the ApplicationUser model directly in the view?
// Because ApplicationUser has fields we don't want on the login form
// (like PasswordHash, SecurityStamp, etc.) and the form would expose
// fields we never intend the user to fill in.
//
// ViewModels are a protective layer between your DB and your UI.
//
// [Required] and [EmailAddress] are "validation attributes" —
// they tell ASP.NET to check these rules BEFORE the form is processed.
// In Django: this is like form validation in forms.py.

using System.ComponentModel.DataAnnotations;

namespace SupplyChainMS.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]   // tells the browser to render this as type="password"
    public string Password { get; set; } = string.Empty;

    // "Remember Me" checkbox — keeps the user logged in longer
    public bool RememberMe { get; set; } = false;
}
