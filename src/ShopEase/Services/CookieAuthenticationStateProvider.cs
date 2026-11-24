using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;

namespace ShopEase.Services;

/// <summary>
/// Tells Blazor who is logged in, by asking the Identity API (GET /manage/info).
/// Based on Microsoft's guide "Secure ASP.NET Core Blazor WebAssembly with ASP.NET Core Identity".
/// </summary>
public class CookieAuthenticationStateProvider(HttpClient http) : AuthenticationStateProvider, IAccountService
{
    private const string ServerUnavailable =
        "Can't reach the login server. Start ShopEase.IdentityApi and try again.";

    private static readonly AuthenticationState Anonymous = new(new ClaimsPrincipal(new ClaimsIdentity()));

    public override Task<AuthenticationState> GetAuthenticationStateAsync() => LoadAuthenticationStateAsync();

    // Asks the Identity API who is logged in. Used by Blazor (above) and after login/logout (below).
    private async Task<AuthenticationState> LoadAuthenticationStateAsync()
    {
        try
        {
            using var response = await http.GetAsync("manage/info");
            if (!response.IsSuccessStatusCode)
            {
                return Anonymous;
            }

            var info = await response.Content.ReadFromJsonAsync<UserInfo>();
            if (string.IsNullOrEmpty(info?.Email))
            {
                return Anonymous;
            }

            var identity = new ClaimsIdentity(
                [new Claim(ClaimTypes.Name, info.Email), new Claim(ClaimTypes.Email, info.Email)],
                authenticationType: "Identity.Cookie");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch (HttpRequestException)
        {
            // The Identity API isn't running: treat the user as logged out.
            return Anonymous;
        }
    }

    public async Task<AuthResult> RegisterAsync(string email, string password)
    {
        try
        {
            using var response = await http.PostAsJsonAsync("register", new { email, password });
            if (response.IsSuccessStatusCode)
            {
                return AuthResult.Success();
            }

            var errors = await ReadErrorsAsync(response);
            return AuthResult.Failure(errors.Length > 0 ? errors : ["Registration failed. Please try again."]);
        }
        catch (HttpRequestException)
        {
            return AuthResult.Failure(ServerUnavailable);
        }
    }

    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        try
        {
            // useCookies=true: the API answers with an HttpOnly cookie instead of a token,
            // so JavaScript on the page (including injected scripts) can't read it.
            using var response = await http.PostAsJsonAsync("login?useCookies=true", new { email, password });
            if (response.IsSuccessStatusCode)
            {
                NotifyAuthenticationStateChanged(LoadAuthenticationStateAsync());
                return AuthResult.Success();
            }

            // Same message for a wrong email or a wrong password, so attackers can't
            // find out which email addresses have accounts.
            return AuthResult.Failure("The email or password is incorrect, or the account is locked for a few minutes.");
        }
        catch (HttpRequestException)
        {
            return AuthResult.Failure(ServerUnavailable);
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            // The API only accepts a JSON body here, which a form on another website can't send.
            using var content = new StringContent("{}", Encoding.UTF8, "application/json");
            using var response = await http.PostAsync("logout", content);
        }
        catch (HttpRequestException)
        {
            // If the API is down there is no session to end.
        }

        NotifyAuthenticationStateChanged(LoadAuthenticationStateAsync());
    }

    // The Identity API returns errors as { "errors": { "PasswordTooShort": ["..."], ... } }.
    private static async Task<string[]> ReadErrorsAsync(HttpResponseMessage response)
    {
        try
        {
            var json = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(json);
            if (!document.RootElement.TryGetProperty("errors", out var errors) || errors.ValueKind != JsonValueKind.Object)
            {
                return [];
            }

            var messages = new List<string>();
            foreach (var property in errors.EnumerateObject())
            {
                if (property.Value.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                foreach (var message in property.Value.EnumerateArray())
                {
                    if (message.GetString() is { Length: > 0 } text)
                    {
                        messages.Add(text);
                    }
                }
            }

            return messages.ToArray();
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private sealed record UserInfo(string? Email, bool IsEmailConfirmed);
}
