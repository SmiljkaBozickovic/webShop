using System.Text;
using System.Text.RegularExpressions;

namespace ShopEase.Core.Security;

/// <summary>
/// Cleans text typed by users before the app stores or shows it (Assignment 4).
/// </summary>
/// <remarks>
/// This is one layer of defence, not the only one:
/// - SQL injection is stopped by parameterised queries (see MySqlCartRepository).
/// - XSS is stopped by Blazor, which HTML-encodes everything written with @value.
///   The app never uses MarkupString, which would turn that protection off.
/// - This class rejects or strips characters a product name never needs,
///   so harmful text doesn't get stored in the first place.
/// </remarks>
public static partial class InputSanitizer
{
    /// <summary>
    /// Allowed characters for product names and categories: letters in any language
    /// (so "Čaša" works), digits, spaces and . , ' &amp; ( ) - / +
    /// Angle brackets, quotes, semicolons and backslashes are not allowed.
    /// </summary>
    public const string SafeTextPattern = @"^[\p{L}\p{N} .,'&()\-/+]*$";

    /// <summary>
    /// Returns a cleaned copy of <paramref name="input"/>: control characters and
    /// HTML tags removed, spaces collapsed, trimmed and cut to <paramref name="maxLength"/>.
    /// </summary>
    public static string Sanitize(string? input, int maxLength = 100)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(input.Length);
        foreach (var c in input.Normalize(NormalizationForm.FormC))
        {
            // Control characters (null bytes, escape codes, ...) never belong in a product name.
            if (!char.IsControl(c))
            {
                builder.Append(c);
            }
        }

        var text = HtmlTagRegex().Replace(builder.ToString(), string.Empty);
        text = text.Replace("<", string.Empty).Replace(">", string.Empty);
        text = WhitespaceRegex().Replace(text, " ").Trim();

        return text.Length > maxLength ? text[..maxLength].TrimEnd() : text;
    }

    /// <summary>True when the text only contains the characters in <see cref="SafeTextPattern"/>.</summary>
    public static bool IsSafeText(string? input) => input is not null && SafeTextRegex().IsMatch(input);

    [GeneratedRegex("<[^>]*>")]
    private static partial Regex HtmlTagRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(SafeTextPattern)]
    private static partial Regex SafeTextRegex();
}
