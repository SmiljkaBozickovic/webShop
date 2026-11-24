using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace ShopEase.Services;

/// <summary>
/// Adds two things to every request the app sends to the Identity API:
/// - "credentials: include", so the browser sends the login cookie.
/// - "X-Requested-With", so the API answers 401 instead of redirecting to a login page.
/// </summary>
public class CookieHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        request.Headers.Add("X-Requested-With", "XMLHttpRequest");
        return base.SendAsync(request, cancellationToken);
    }
}
