# ShopEase – e-commerce with Blazor

Course project covering Assignments 1–4: product and cart classes with MySQL,
a Blazor WebAssembly shop, responsive and accessible styling, and secure login.

## What you need

- **.NET 11 SDK** – check with `dotnet --list-sdks` (needs a line starting with `11.`).
  The project targets .NET 11 (`net11.0` in each `.csproj`).
- **MySQL Server** running on `localhost:3306` (for Assignment 1).
- **VS Code** with the *C# Dev Kit* extension.
- A trusted HTTPS certificate for local development (run this once):

  ```
  dotnet dev-certs https --trust
  ```

## Project layout

| Folder | What it is |
|---|---|
| `src/ShopEase` | The Blazor WebAssembly app (Assignments 2–4) |
| `src/ShopEase.Core` | `Product`, `Cart`, input validation – shared by everything |
| `src/ShopEase.Data` | `MySqlCartRepository` – saves the cart to MySQL |
| `src/ShopEase.ConsoleTest` | The Assignment 1 test program |
| `src/ShopEase.IdentityApi` | Login server with ASP.NET Core Identity (Assignment 4) |
| `tests/ShopEase.Tests` | Unit and security tests |
| `database/shop.sql` | The `Shop` database and `Products` table (optional – created automatically) |

Open a terminal in this folder (in VS Code: **Terminal → New Terminal**) for the commands below.

---

## Assignment 1 – Product and Cart classes

**Files:** `src/ShopEase.Core/Product.cs`, `src/ShopEase.Core/Cart.cs`,
`src/ShopEase.Core/ICartRepository.cs`, `src/ShopEase.Data/MySqlCartRepository.cs`,
`src/ShopEase.ConsoleTest/Program.cs`

- `Product` has `ProductID`, `Name`, `Price`, `Category`, and `PrintDetails()`, which prints
  `Product: Laptop | Price: $999.99 | Category: Electronics`.
- `Cart` keeps a `List<Product>` and has `AddProduct`, `RemoveProduct`, `DisplayCartItems`
  and `CalculateTotal`.
- **Database:** when a `Cart` is created with a `MySqlCartRepository`, `AddProduct` inserts a row
  into `Shop.Products` and `RemoveProduct` deletes it. The program creates the database and
  table if they don't exist.

Run the test program:

```
dotnet run --project src/ShopEase.ConsoleTest
```

It asks for your MySQL user (press Enter for `root`) and password. Expected output:

```
=== Step 2: Product test ===
Product: Laptop | Price: $999.99 | Category: Electronics

=== Step 3: Cart test (MySQL database 'Shop', table 'Products') ===
Connected to MySQL. Database 'Shop' and table 'Products' are ready.

Added 3 products. Items in cart: 3
Removed product 2 (Wireless Headphones).

Cart items (2):
  Product: Laptop | Price: $999.99 | Category: Electronics
  Product: Coffee Mug | Price: $12.99 | Category: Kitchen
Total: $1,012.98

Rows in Shop.Products: 2
  ...
```

You can also check in MySQL Workbench with `SELECT * FROM Shop.Products;`.

**Why the Blazor app doesn't use MySQL:** Blazor WebAssembly runs inside the browser, and a
browser can't open a MySQL connection. So the app uses the same `Cart` class without a
database, and the MySQL part is shown in the console program.

---

## Assignment 2 – ProductCard component

**Files:** `src/ShopEase/Pages/ProductCard.razor`, `src/ShopEase/Pages/CartView.razor`,
`src/ShopEase/Pages/Home.razor`

- `ProductCard.razor` takes a `Product` parameter and shows its details and an **Add to Cart** button.
- Clicking the button raises the `OnAddToCart` event (`EventCallback<Product>`).
- `Home.razor` shows six products and handles the event by calling `Cart.AddProduct`.
- `CartView.razor` (the cart component) listens to `Cart.CartChanged` and redraws itself
  with the items and total.

---

## Assignment 3 – Styling, responsive design, accessibility

**File:** `src/ShopEase/wwwroot/css/site.css` (linked from `wwwroot/index.html`)

- **Layout:** one column on phones, two from 600px (tablet), three plus a cart sidebar
  that stays visible from 1024px (desktop).
- **Contrast:** every text colour is at least 4.5:1 against its background (most are above 6:1).
- **Keyboard:** all controls are real buttons, links and inputs; a thick outline shows which
  one has focus; a "Skip to main content" link appears on the first Tab press.
- **Screen readers:** form labels, `aria-describedby` on the Add to Cart buttons, and a live
  region that announces cart changes.
- Respects "reduce motion" and Windows High Contrast settings.

To test responsiveness: open the app, press **F12**, click the device toolbar icon
(**Ctrl+Shift+M**) and try a phone, a tablet and a desktop width.

---

## Assignment 4 – Security

**Files:** `src/ShopEase.Core/Security/InputSanitizer.cs`, `src/ShopEase.Core/ProductInput.cs`,
`src/ShopEase/Pages/AddProductForm.razor`, `src/ShopEase/Pages/Login.razor`,
`src/ShopEase/Pages/Register.razor`, `src/ShopEase/Services/*`, `src/ShopEase.IdentityApi/Program.cs`,
`tests/ShopEase.Tests/SecurityTests.cs`

**Input validation (Step 1)** – the "Add a product" form (shown after login):
- Validation rules on `ProductInput`: required, length limits, a price range, and an allow-list
  of characters (letters in any language, digits, spaces and `. , ' & ( ) - / +`).
- `InputSanitizer` also strips HTML tags and control characters before a product is created.
- **SQL injection:** every database query uses parameters (`@id`, `@name`, …), so typed text
  is never run as SQL.
- **XSS:** Blazor HTML-encodes all text it displays, and the app never uses `MarkupString`.

**Authentication (Step 2)** – ASP.NET Core Identity:
- Blazor WebAssembly can't store passwords in the browser, so `ShopEase.IdentityApi` is a
  small server that does it. This is the setup Microsoft recommends for WebAssembly apps.
  Users are saved in `src/ShopEase.IdentityApi/identity.db` (SQLite), which is created on first run.
- Only logged-in users see the **Add to Cart** button, and `Home.razor` checks again before
  adding to the cart.
- The login cookie is `HttpOnly`, `Secure` and `SameSite=Lax`; only the ShopEase app is allowed
  to call the API (CORS); accounts lock for 5 minutes after 5 wrong passwords; passwords need
  8+ characters with upper and lower case letters, a digit and a symbol; the login page refuses
  return links to other websites.

**Security tests (Step 3):**

```
dotnet test
```

The tests try typical attack strings (`<script>…`, `' OR '1'='1`, `; DROP TABLE …`) against
the form and check that they are rejected.

---

## Running the Blazor app

You need **two terminals** – one for the login server and one for the app.

Terminal 1 – login server:

```
dotnet run --project src/ShopEase.IdentityApi
```

Wait for `Now listening on: https://localhost:7080`.

Terminal 2 – the shop:

```
dotnet run --project src/ShopEase
```

Then open **https://localhost:7292**. Register an account, add products to the cart, and try
the "Add a product" form with something like `<script>alert(1)</script>`.

If you change the ports, update `src/ShopEase/wwwroot/appsettings.json` (`IdentityApiUrl`) and
`src/ShopEase.IdentityApi/appsettings.json` (`FrontendOrigins`).

## Troubleshooting

| Problem | Fix |
|---|---|
| "Can't reach the login server" | Start `ShopEase.IdentityApi` first (Terminal 1). |
| Login works but you stay logged out | Open the app at **https**://localhost:7292, not http. |
| Browser warns about the certificate | Run `dotnet dev-certs https --trust` and restart the browser. |
| `Access denied for user 'root'@'localhost'` | Wrong MySQL password, or use another MySQL user. |
| `Unable to connect to any of the specified MySQL hosts` | Start MySQL (Windows: **Services → MySQL80 → Start**). |
| `NETSDK1045: The current .NET SDK does not support targeting .NET 11.0` | Install the .NET 11 SDK (see "What you need"). |
