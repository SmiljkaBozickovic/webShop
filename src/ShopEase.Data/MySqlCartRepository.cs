using System.Text.RegularExpressions;
using MySqlConnector;
using ShopEase.Core;

namespace ShopEase.Data;

/// <summary>
/// Saves cart items to the local MySQL database "Shop", table "Products"
/// (Assignment 1, Step 3: "Include a database component in your Cart methods").
/// </summary>
/// <remarks>
/// Every query uses parameters (@id, @name, ...). Values are sent separately from
/// the SQL text, so a product name such as "x'); DROP TABLE Products; --" is stored
/// as plain text and can never run as SQL. This is the main defence against SQL
/// injection (Assignment 4).
/// </remarks>
public class MySqlCartRepository : ICartRepository
{
    private const string CreateTableSql = """
        CREATE TABLE IF NOT EXISTS Products (
            CartItemID INT AUTO_INCREMENT PRIMARY KEY,
            ProductID  INT            NOT NULL,
            Name       VARCHAR(100)   NOT NULL,
            Price      DECIMAL(10, 2) NOT NULL,
            Category   VARCHAR(50)    NOT NULL,
            AddedAt    DATETIME       NOT NULL DEFAULT CURRENT_TIMESTAMP,
            INDEX IX_Products_ProductID (ProductID)
        );
        """;

    private readonly string _connectionString;

    public MySqlCartRepository(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("A MySQL connection string is required.", nameof(connectionString));
        }

        _connectionString = connectionString;
    }

    /// <summary>
    /// Creates the "Shop" database and the "Products" table if they don't exist yet.
    /// </summary>
    public void EnsureDatabase()
    {
        var builder = new MySqlConnectionStringBuilder(_connectionString);
        var databaseName = string.IsNullOrWhiteSpace(builder.Database) ? "Shop" : builder.Database;

        // A database name can't be sent as a parameter, so only allow letters, digits and _.
        if (!Regex.IsMatch(databaseName, "^[A-Za-z0-9_]{1,64}$"))
        {
            throw new ArgumentException($"'{databaseName}' is not a valid database name.");
        }

        // Connect without choosing a database, because "Shop" may not exist yet.
        builder.Database = string.Empty;
        using (var serverConnection = new MySqlConnection(builder.ConnectionString))
        {
            serverConnection.Open();
            using var createDatabase = new MySqlCommand($"CREATE DATABASE IF NOT EXISTS `{databaseName}`;", serverConnection);
            createDatabase.ExecuteNonQuery();
        }

        builder.Database = databaseName;
        using var connection = new MySqlConnection(builder.ConnectionString);
        connection.Open();
        using var createTable = new MySqlCommand(CreateTableSql, connection);
        createTable.ExecuteNonQuery();
    }

    public void SaveItem(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);

        using var connection = OpenConnection();
        using var command = new MySqlCommand(
            "INSERT INTO Products (ProductID, Name, Price, Category) VALUES (@id, @name, @price, @category);",
            connection);
        command.Parameters.AddWithValue("@id", product.ProductID);
        command.Parameters.AddWithValue("@name", product.Name);
        command.Parameters.AddWithValue("@price", product.Price);
        command.Parameters.AddWithValue("@category", product.Category);
        command.ExecuteNonQuery();
    }

    public void RemoveItems(int productId)
    {
        using var connection = OpenConnection();
        using var command = new MySqlCommand("DELETE FROM Products WHERE ProductID = @id;", connection);
        command.Parameters.AddWithValue("@id", productId);
        command.ExecuteNonQuery();
    }

    public void Clear()
    {
        using var connection = OpenConnection();
        using var command = new MySqlCommand("DELETE FROM Products;", connection);
        command.ExecuteNonQuery();
    }

    /// <summary>Reads back the cart items saved in the database (used by the test program).</summary>
    public List<Product> GetItems()
    {
        var items = new List<Product>();

        using var connection = OpenConnection();
        using var command = new MySqlCommand(
            "SELECT ProductID, Name, Price, Category FROM Products ORDER BY CartItemID;",
            connection);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            items.Add(new Product(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetDecimal(2),
                reader.GetString(3)));
        }

        return items;
    }

    private MySqlConnection OpenConnection()
    {
        var connection = new MySqlConnection(_connectionString);
        connection.Open();
        return connection;
    }
}
