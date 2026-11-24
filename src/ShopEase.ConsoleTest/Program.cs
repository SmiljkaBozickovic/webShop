// ShopEase – Assignment 1 test program.
// Step 2: creates a Product and prints its details.
// Step 3: adds products to a Cart (saved in MySQL), removes one, then shows the cart and total.
//
// Run it from the project folder with:  dotnet run --project src/ShopEase.ConsoleTest
// It asks for your MySQL user and password. To skip the questions, set the
// SHOPEASE_DB environment variable to a full connection string first.

using MySqlConnector;
using ShopEase.Core;
using ShopEase.Data;

Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.WriteLine("=== Step 2: Product test ===");
var laptop = new Product(1, "Laptop", 999.99m, "Electronics");
laptop.PrintDetails();
Console.WriteLine();

Console.WriteLine("=== Step 3: Cart test (MySQL database 'Shop', table 'Products') ===");
var connectionString = Environment.GetEnvironmentVariable("SHOPEASE_DB") ?? AskForConnectionString();

try
{
    var repository = new MySqlCartRepository(connectionString);
    repository.EnsureDatabase();
    repository.Clear(); // start each test run with an empty cart table
    Console.WriteLine("Connected to MySQL. Database 'Shop' and table 'Products' are ready.");
    Console.WriteLine();

    var cart = new Cart(repository);

    var headphones = new Product(2, "Wireless Headphones", 149.50m, "Electronics");
    var mug = new Product(3, "Coffee Mug", 12.99m, "Kitchen");

    cart.AddProduct(laptop);
    cart.AddProduct(headphones);
    cart.AddProduct(mug);
    Console.WriteLine($"Added 3 products. Items in cart: {cart.Count}");

    var removed = cart.RemoveProduct(headphones.ProductID);
    Console.WriteLine(removed
        ? $"Removed product {headphones.ProductID} ({headphones.Name})."
        : $"Product {headphones.ProductID} was not in the cart.");
    Console.WriteLine();

    cart.DisplayCartItems();
    Console.WriteLine($"Total: {cart.FormattedTotal}");
    Console.WriteLine();

    // Read the rows back from MySQL to prove the cart was saved there.
    var savedItems = repository.GetItems();
    Console.WriteLine($"Rows in Shop.Products: {savedItems.Count}");
    foreach (var item in savedItems)
    {
        Console.WriteLine($"  {item.GetDetails()}");
    }

    return 0;
}
catch (MySqlException ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"MySQL error: {ex.Message}");
    Console.ResetColor();
    Console.WriteLine("Check that the MySQL server is running and that the user name and password are correct.");
    return 1;
}

static string AskForConnectionString()
{
    Console.Write("MySQL user [root]: ");
    var user = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(user))
    {
        user = "root";
    }

    Console.Write("MySQL password: ");
    var password = ReadPassword();

    var builder = new MySqlConnectionStringBuilder
    {
        Server = "localhost",
        Port = 3306,
        UserID = user.Trim(),
        Password = password,
        Database = "Shop",
    };
    return builder.ConnectionString;
}

// Reads a password without showing it on screen.
static string ReadPassword()
{
    if (Console.IsInputRedirected)
    {
        return Console.ReadLine() ?? string.Empty;
    }

    var password = new System.Text.StringBuilder();
    while (true)
    {
        var key = Console.ReadKey(intercept: true);
        if (key.Key == ConsoleKey.Enter)
        {
            Console.WriteLine();
            return password.ToString();
        }

        if (key.Key == ConsoleKey.Backspace)
        {
            if (password.Length > 0)
            {
                password.Length--;
            }
        }
        else if (!char.IsControl(key.KeyChar))
        {
            password.Append(key.KeyChar);
        }
    }
}
