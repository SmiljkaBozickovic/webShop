namespace ShopEase.Core;

/// <summary>
/// The products shown in the shop. It starts with a few sample products;
/// logged-in users can add more with the "Add a product" form.
/// </summary>
public class ProductCatalog
{
    private readonly List<Product> _products =
    [
        new Product(1, "Laptop", 999.99m, "Electronics"),
        new Product(2, "Wireless Headphones", 149.50m, "Electronics"),
        new Product(3, "Coffee Mug", 12.99m, "Kitchen"),
        new Product(4, "Running Shoes", 89.90m, "Sports"),
        new Product(5, "Desk Lamp", 34.95m, "Home"),
        new Product(6, "Backpack", 59.00m, "Accessories"),
    ];

    /// <summary>Raised after a product is added, so pages can redraw.</summary>
    public event Action? CatalogChanged;

    public IReadOnlyList<Product> Products => _products.AsReadOnly();

    /// <summary>Checks and cleans the input, then adds it as a new product with the next free ID.</summary>
    public Product Add(ProductInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var nextId = _products.Count == 0 ? 1 : _products.Max(p => p.ProductID) + 1;
        var product = input.ToProduct(nextId);
        _products.Add(product);
        CatalogChanged?.Invoke();
        return product;
    }
}
