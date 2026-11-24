using System.Globalization;

namespace ShopEase.Core;

/// <summary>
/// A shopping cart that holds products and calculates the total (Assignment 1, Step 3).
/// </summary>
public class Cart
{
    private static readonly CultureInfo PriceCulture = CultureInfo.GetCultureInfo("en-US");

    private readonly List<Product> _products = new();
    private readonly ICartRepository? _repository;

    /// <summary>Creates a cart that keeps its items in memory only.</summary>
    public Cart()
        : this(null)
    {
    }

    /// <summary>
    /// Creates a cart that also saves its items through <paramref name="repository"/>,
    /// for example to the MySQL database "Shop".
    /// </summary>
    public Cart(ICartRepository? repository)
    {
        _repository = repository;
    }

    /// <summary>Raised after the cart changes, so the Blazor page can redraw itself.</summary>
    public event Action? CartChanged;

    /// <summary>The products in the cart (read-only, so other code can't bypass the methods below).</summary>
    public IReadOnlyList<Product> Products => _products.AsReadOnly();

    /// <summary>The number of items in the cart.</summary>
    public int Count => _products.Count;

    /// <summary>Adds a product to the cart and saves it to the database.</summary>
    public void AddProduct(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);

        if (product.Price < 0)
        {
            throw new ArgumentException("A product's price can't be negative.", nameof(product));
        }

        // Save to the database first. If saving fails, an exception is thrown and the
        // product is not added to the list, so the list and the database stay the same.
        _repository?.SaveItem(product);
        _products.Add(product);
        CartChanged?.Invoke();
    }

    /// <summary>
    /// Removes a product from the cart by its ID, and from the database.
    /// If the product was added more than once, every copy is removed.
    /// </summary>
    /// <returns>True if something was removed; false if the product wasn't in the cart.</returns>
    public bool RemoveProduct(int productId)
    {
        if (!_products.Exists(p => p.ProductID == productId))
        {
            return false;
        }

        _repository?.RemoveItems(productId);
        _products.RemoveAll(p => p.ProductID == productId);
        CartChanged?.Invoke();
        return true;
    }

    /// <summary>Removes everything from the cart and the database.</summary>
    public void Clear()
    {
        _repository?.Clear();
        _products.Clear();
        CartChanged?.Invoke();
    }

    /// <summary>Prints all products in the cart.</summary>
    public void DisplayCartItems()
    {
        if (_products.Count == 0)
        {
            Console.WriteLine("The cart is empty.");
            return;
        }

        Console.WriteLine($"Cart items ({_products.Count}):");
        foreach (var product in _products)
        {
            Console.WriteLine($"  {product.GetDetails()}");
        }
    }

    /// <summary>Returns the total price of all products in the cart.</summary>
    public decimal CalculateTotal()
    {
        decimal total = 0m;
        foreach (var product in _products)
        {
            total += product.Price;
        }

        return total;
    }

    /// <summary>The total formatted for display, for example "$1,049.98".</summary>
    public string FormattedTotal => CalculateTotal().ToString("C2", PriceCulture);
}
