using System.Globalization;

namespace ShopEase.Core;

/// <summary>
/// A product that customers can browse and add to their cart (Assignment 1, Step 2).
/// </summary>
public class Product
{
    // Prices are always shown as US dollars ("$999.99"), whatever language the
    // computer or browser uses. Without this, a Croatian system would print "999,99 $".
    private static readonly CultureInfo PriceCulture = CultureInfo.GetCultureInfo("en-US");

    public int ProductID { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string Category { get; set; } = string.Empty;

    public Product()
    {
    }

    public Product(int productId, string name, decimal price, string category)
    {
        ProductID = productId;
        Name = name;
        Price = price;
        Category = category;
    }

    /// <summary>The price formatted for display, for example "$999.99".</summary>
    public string FormattedPrice => Price.ToString("C2", PriceCulture);

    /// <summary>
    /// Returns the product details in the required format:
    /// Product: Laptop | Price: $999.99 | Category: Electronics
    /// </summary>
    public string GetDetails() => $"Product: {Name} | Price: {FormattedPrice} | Category: {Category}";

    /// <summary>Prints the product details to the console.</summary>
    public void PrintDetails() => Console.WriteLine(GetDetails());

    public override string ToString() => GetDetails();
}
