using System.ComponentModel.DataAnnotations;

namespace ShopEase.Models;

/// <summary>The login form.</summary>
public class LoginModel
{
    [Required(ErrorMessage = "Enter your email address.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Enter your password.")]
    [StringLength(100)]
    public string Password { get; set; } = string.Empty;
}

/// <summary>The registration form.</summary>
public class RegisterModel
{
    [Required(ErrorMessage = "Enter your email address.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    // The Identity API checks the full password rules too (see ShopEase.IdentityApi/Program.cs).
    [Required(ErrorMessage = "Enter a password.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "The password must be at least 8 characters long.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Enter the password again.")]
    [Compare(nameof(Password), ErrorMessage = "The passwords don't match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
