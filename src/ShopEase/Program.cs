using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ShopEase;
using ShopEase.Core;
using ShopEase.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Shop data. In Blazor WebAssembly a "scoped" service lives as long as the browser tab,
// so every page shares the same cart. The cart has no database here: code in the
// browser can't open a MySQL connection (see ShopEase.ConsoleTest for the MySQL version).
builder.Services.AddScoped<Cart>();
builder.Services.AddScoped<ProductCatalog>();

// Assignment 4: log in with ASP.NET Core Identity.
// The Identity API (ShopEase.IdentityApi) issues a secure, HttpOnly cookie;
// CookieHandler makes the browser send that cookie with every request to the API.
var identityApiUrl = builder.Configuration["IdentityApiUrl"] ?? "https://localhost:7080";

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped(_ => new HttpClient(new CookieHandler { InnerHandler = new HttpClientHandler() })
{
    BaseAddress = new Uri(identityApiUrl),
});
builder.Services.AddScoped<CookieAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CookieAuthenticationStateProvider>());
builder.Services.AddScoped<IAccountService>(sp => sp.GetRequiredService<CookieAuthenticationStateProvider>());

await builder.Build().RunAsync();
