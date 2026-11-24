using ShopEase.Core;

namespace ShopEase.Tests;

public class CartTests
{
    private static Product Laptop() => new(1, "Laptop", 999.99m, "Electronics");

    private static Product Mug() => new(3, "Coffee Mug", 12.99m, "Kitchen");

    [Fact]
    public void Product_details_use_the_required_format()
    {
        Assert.Equal("Product: Laptop | Price: $999.99 | Category: Electronics", Laptop().GetDetails());
    }

    [Fact]
    public void AddProduct_adds_to_the_list_and_saves_to_the_repository()
    {
        var repository = new FakeCartRepository();
        var cart = new Cart(repository);

        cart.AddProduct(Laptop());
        cart.AddProduct(Mug());

        Assert.Equal(2, cart.Count);
        Assert.Equal(new[] { 1, 3 }, repository.SavedIds);
    }

    [Fact]
    public void RemoveProduct_removes_from_the_list_and_the_repository()
    {
        var repository = new FakeCartRepository();
        var cart = new Cart(repository);
        cart.AddProduct(Laptop());
        cart.AddProduct(Mug());

        var removed = cart.RemoveProduct(1);

        Assert.True(removed);
        Assert.Single(cart.Products);
        Assert.Equal(new[] { 1 }, repository.RemovedIds);
    }

    [Fact]
    public void RemoveProduct_returns_false_for_a_product_not_in_the_cart()
    {
        var repository = new FakeCartRepository();
        var cart = new Cart(repository);

        Assert.False(cart.RemoveProduct(42));
        Assert.Empty(repository.RemovedIds);
    }

    [Fact]
    public void CalculateTotal_adds_up_all_prices()
    {
        var cart = new Cart();
        cart.AddProduct(Laptop());
        cart.AddProduct(Mug());
        cart.AddProduct(Mug());

        Assert.Equal(1025.97m, cart.CalculateTotal());
        Assert.Equal("$1,025.97", cart.FormattedTotal);
    }

    [Fact]
    public void Empty_cart_total_is_zero()
    {
        Assert.Equal(0m, new Cart().CalculateTotal());
    }

    [Fact]
    public void AddProduct_rejects_null_and_negative_prices()
    {
        var cart = new Cart();

        Assert.Throws<ArgumentNullException>(() => cart.AddProduct(null!));
        Assert.Throws<ArgumentException>(() => cart.AddProduct(new Product(9, "Broken", -1m, "Test")));
        Assert.Equal(0, cart.Count);
    }

    [Fact]
    public void Product_is_not_added_when_saving_to_the_database_fails()
    {
        var cart = new Cart(new FakeCartRepository { FailOnSave = true });

        Assert.Throws<InvalidOperationException>(() => cart.AddProduct(Laptop()));
        Assert.Equal(0, cart.Count);
    }

    [Fact]
    public void CartChanged_is_raised_when_the_cart_changes()
    {
        var cart = new Cart();
        var changes = 0;
        cart.CartChanged += () => changes++;

        cart.AddProduct(Laptop());
        cart.RemoveProduct(1);
        cart.Clear();

        Assert.Equal(3, changes);
    }

    private sealed class FakeCartRepository : ICartRepository
    {
        public bool FailOnSave { get; init; }

        public List<int> SavedIds { get; } = new();

        public List<int> RemovedIds { get; } = new();

        public void SaveItem(Product product)
        {
            if (FailOnSave)
            {
                throw new InvalidOperationException("Database is down.");
            }

            SavedIds.Add(product.ProductID);
        }

        public void RemoveItems(int productId) => RemovedIds.Add(productId);

        public void Clear()
        {
            SavedIds.Clear();
        }
    }
}
