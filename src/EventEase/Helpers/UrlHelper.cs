using System.Text.RegularExpressions;

namespace EventEase.Helpers;

public static partial class UrlHelper
{
    /// <summary>
    /// Turns a "returnUrl" query value into a safe, base-relative path.
    /// Anything that could leave the app (other hosts, "javascript:" and similar schemes)
    /// falls back to the home page, preventing open redirects.
    /// </summary>
    public static string GetSafeReturnUrl(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl))
        {
            return string.Empty;
        }

        var url = returnUrl.Trim();

        // "//host" and "/\host" are protocol-relative URLs to another site.
        if (url.StartsWith("//", StringComparison.Ordinal)
            || url.StartsWith("/\\", StringComparison.Ordinal)
            || url.StartsWith('\\')
            || SchemePrefix().IsMatch(url))
        {
            return string.Empty;
        }

        // Base-relative path, so the app also works when hosted in a sub-folder.
        return url.TrimStart('/');
    }

    [GeneratedRegex(@"^[a-zA-Z][a-zA-Z0-9+.\-]*:")]
    private static partial Regex SchemePrefix();
}
