// ShopEase.IdentityApi – Assignment 4, Step 2: user accounts with ASP.NET Core Identity.
//
// Blazor WebAssembly runs in the browser, so it can't keep passwords or user data itself.
// This small API does that job. It follows Microsoft's guide
// "Secure ASP.NET Core Blazor WebAssembly with ASP.NET Core Identity".
//
// Endpoints (added by MapIdentityApi): POST /register, POST /login, GET /manage/info, ...
// plus POST /logout below. Users are stored in identity.db (SQLite), created on first run.

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Identity") ?? "Data Source=identity.db"));

builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme).AddIdentityCookies();
builder.Services.AddAuthorizationBuilder();

builder.Services.AddIdentityCore<AppUser>(options =>
    {
        options.User.RequireUniqueEmail = true;

        // Password rules.
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;

        // Lock the account for 5 minutes after 5 wrong passwords (slows down password guessing).
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddApiEndpoints();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "ShopEase.Auth";
    options.Cookie.HttpOnly = true;                          // JavaScript can't read the cookie (limits XSS damage)
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // only sent over HTTPS
    options.Cookie.SameSite = SameSiteMode.Lax;              // not sent with requests started by other websites
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;

    // This is an API: answer 401/403 instead of redirecting to a login page.
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});

// Only the ShopEase Blazor app may call this API from a browser.
var frontendOrigins = builder.Configuration.GetSection("FrontendOrigins").Get<string[]>() ?? ["https://localhost:7292"];
builder.Services.AddCors(options => options.AddPolicy("ShopEaseFrontend", policy => policy
    .WithOrigins(frontendOrigins)
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials()));

var app = builder.Build();

// Create identity.db and its tables the first time the API starts.
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors("ShopEaseFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "ShopEase Identity API is running.");

app.MapIdentityApi<AppUser>();

// Logging out needs a JSON body. A form on another website can't send JSON without
// the browser asking this API first (CORS), so other sites can't log users out (CSRF).
app.MapPost("/logout", async (SignInManager<AppUser> signInManager, [FromBody] object? empty) =>
    {
        if (empty is null)
        {
            return Results.BadRequest();
        }

        await signInManager.SignOutAsync();
        return Results.Ok();
    })
    .RequireAuthorization();

app.Run();

/// <summary>A ShopEase user account.</summary>
public class AppUser : IdentityUser
{
}

/// <summary>The database that stores user accounts (identity.db).</summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<AppUser>(options)
{
}
