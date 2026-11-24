using System.ComponentModel.DataAnnotations;
using ShopEase.Core;
using ShopEase.Core.Security;

namespace ShopEase.Tests;

/// <summary>
/// Assignment 4, Step 3: checks that input validation stops typical attack strings.
/// </summary>
public class SecurityTests
{
    public static TheoryData<string> AttackStrings => new()
    {
        "<script>alert('xss')</script>",
        "<img src=x onerror=alert(1)>",
        "\"><svg onload=alert(1)>",
        "javascript:alert(1)",
        "Robert'); DROP TABLE Products; --",
        "1' OR '1'='1",
        "Laptop; DELETE FROM Products",
        "Laptop\\",
        "admin\" --",
    };

    [Theory]
    [MemberData(nameof(AttackStrings))]
    public void Form_rejects_attack_strings_in_the_product_name(string attack)
    {
        var input = new ProductInput { Name = attack, Category = "Electronics", Price = 10m };

        var errors = Validate(input);

        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(ProductInput.Name)));
    }

    [Theory]
    [MemberData(nameof(AttackStrings))]
    public void Form_rejects_attack_strings_in_the_category(string attack)
    {
        var input = new ProductInput { Name = "Laptop", Category = attack, Price = 10m };

        var errors = Validate(input);

        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(ProductInput.Category)));
    }

    [Theory]
    [MemberData(nameof(AttackStrings))]
    public void ToProduct_refuses_attack_strings_even_when_validation_is_skipped(string attack)
    {
        var input = new ProductInput { Name = attack, Category = "Electronics", Price = 10m };

        // Either the text is cleaned into something harmless, or ToProduct refuses it.
        try
        {
            var product = input.ToProduct(1);
            Assert.True(InputSanitizer.IsSafeText(product.Name));
            Assert.DoesNotContain("<", product.Name);
            Assert.DoesNotContain(";", product.Name);
        }
        catch (ValidationException)
        {
            // Refused: also fine.
        }
    }

    [Theory]
    [InlineData("Laptop Pro 14\"", false)]
    [InlineData("Men's Running Shoes", true)]
    [InlineData("Čaša za kavu", true)]
    [InlineData("USB-C Cable (2 m)", true)]
    [InlineData("Salt & Pepper", true)]
    public void Product_names_are_checked(string name, bool expectedValid)
    {
        var input = new ProductInput { Name = name, Category = "Home", Price = 10m };

        var isValid = Validate(input).Count == 0;

        Assert.Equal(expectedValid, isValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(100000.01)]
    public void Prices_outside_the_allowed_range_are_rejected(decimal price)
    {
        var input = new ProductInput { Name = "Laptop", Category = "Electronics", Price = price };

        Assert.Contains(Validate(input), e => e.MemberNames.Contains(nameof(ProductInput.Price)));
    }

    [Fact]
    public void Empty_fields_are_rejected()
    {
        var errors = Validate(new ProductInput());

        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(ProductInput.Name)));
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(ProductInput.Category)));
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(ProductInput.Price)));
    }

    [Fact]
    public void Sanitize_removes_tags_control_characters_and_extra_spaces()
    {
        Assert.Equal("Hello world", InputSanitizer.Sanitize("  <b>Hello</b>\u0000   world  "));
        Assert.Equal("alert(1)", InputSanitizer.Sanitize("<script>alert(1)</script>"));
        Assert.Equal(string.Empty, InputSanitizer.Sanitize(null));
    }

    [Fact]
    public void Sanitize_cuts_long_text()
    {
        Assert.Equal(10, InputSanitizer.Sanitize(new string('a', 500), maxLength: 10).Length);
    }

    [Fact]
    public void Catalog_adds_a_valid_product_with_the_next_id()
    {
        var catalog = new ProductCatalog();
        var before = catalog.Products.Count;

        var product = catalog.Add(new ProductInput { Name = "  Desk   Chair ", Category = "Home", Price = 129.999m });

        Assert.Equal(before + 1, catalog.Products.Count);
        Assert.Equal("Desk Chair", product.Name);
        Assert.Equal(130.00m, product.Price);
        Assert.Equal(catalog.Products.Max(p => p.ProductID), product.ProductID);
    }

    private static List<ValidationResult> Validate(object model)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);
        return results;
    }
}
