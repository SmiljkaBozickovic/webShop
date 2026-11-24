namespace ShopEase.Services;

/// <summary>Register, log in and log out through the Identity API.</summary>
public interface IAccountService
{
    Task<AuthResult> RegisterAsync(string email, string password);

    Task<AuthResult> LoginAsync(string email, string password);

    Task LogoutAsync();
}

/// <summary>The outcome of a register or login attempt.</summary>
public sealed record AuthResult(bool Succeeded, IReadOnlyList<string> Errors)
{
    public static AuthResult Success() => new(true, []);

    public static AuthResult Failure(params string[] errors) => new(false, errors);
}
