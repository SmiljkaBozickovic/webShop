using System.ComponentModel.DataAnnotations;
using ShopEase.Core.Security;

namespace ShopEase.Core;

/// <summary>
/// What the user types into the "Add a product" form (Assignment 4, Step 1).
/// The attributes are checked by Blazor's DataAnnotationsValidator before the form submits.
/// </summary>
public class ProductInput
{
    public const int NameMaxLength = 100;
    public const int CategoryMaxLength = 50;

    [Required(ErrorMessage = "Enter a product name.")]
    [StringLength(NameMaxLength, MinimumLength = 2, ErrorMessage = "The name must be 2 to 100 characters long.")]
    [RegularExpression(InputSanitizer.SafeTextPattern,
        ErrorMessage = "The name can only use letters, numbers, spaces and . , ' & ( ) - / +")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Enter a category.")]
    [StringLength(CategoryMaxLength, MinimumLength = 2, ErrorMessage = "The category must be 2 to 50 characters long.")]
    [RegularExpression(InputSanitizer.SafeTextPattern,
        ErrorMessage = "The category can only use letters, numbers, spaces and . , ' & ( ) - / +")]
    public string Category { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "100000",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true,
        ErrorMessage = "Enter a price from $0.01 to $100,000.")]
    public decimal Price { get; set; }

    /// <summary>
    /// Turns the checked input into a Product. The text is sanitized again here,
    /// so a product is never created from unchecked text even if validation was skipped.
    /// </summary>
    public Product ToProduct(int productId)
    {
        var name = InputSanitizer.Sanitize(Name, NameMaxLength);
        var category = InputSanitizer.Sanitize(Category, CategoryMaxLength);

        if (name.Length < 2 || category.Length < 2 || !InputSanitizer.IsSafeText(name) || !InputSanitizer.IsSafeText(category))
        {
            throw new ValidationException("The product name or category contains characters that aren't allowed.");
        }

        if (Price < 0.01m || Price > 100_000m)
        {
            throw new ValidationException("The price must be from $0.01 to $100,000.");
        }

        return new Product(productId, name, decimal.Round(Price, 2), category);
    }
}
