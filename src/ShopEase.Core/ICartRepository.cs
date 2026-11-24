namespace ShopEase.Core;

/// <summary>
/// Where the cart saves its items. The console test program uses the MySQL
/// version (ShopEase.Data.MySqlCartRepository). The Blazor app runs in the
/// browser, which cannot open a MySQL connection, so it uses the cart without one.
/// </summary>
public interface ICartRepository
{
    /// <summary>Saves one cart item.</summary>
    void SaveItem(Product product);

    /// <summary>Removes every cart item with this product ID.</summary>
    void RemoveItems(int productId);

    /// <summary>Removes all cart items.</summary>
    void Clear();
}
