using EventEase.Helpers;

namespace EventEase.Tests;

public class UrlHelperTests
{
    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("my-events", "my-events")]
    [InlineData("/events/3", "events/3")]
    [InlineData("events?category=Workshop", "events?category=Workshop")]
    public void LocalUrls_AreKeptAsBaseRelativePaths(string? returnUrl, string expected)
    {
        Assert.Equal(expected, UrlHelper.GetSafeReturnUrl(returnUrl));
    }

    [Theory]
    [InlineData("https://evil.example")]
    [InlineData("//evil.example")]
    [InlineData("/\\evil.example")]
    [InlineData("\\\\evil.example")]
    [InlineData("javascript:alert(1)")]
    public void ExternalOrScriptUrls_FallBackToHome(string returnUrl)
    {
        Assert.Equal(string.Empty, UrlHelper.GetSafeReturnUrl(returnUrl));
    }
}
