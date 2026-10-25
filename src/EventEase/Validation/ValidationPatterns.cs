namespace EventEase.Validation;

public static class ValidationPatterns
{
    /// <summary>Letters (any alphabet), spaces, apostrophes, dots and hyphens; must start with a letter.</summary>
    public const string PersonName = @"^\p{L}[\p{L}\s'.-]*$";

    /// <summary>
    /// Stricter than [EmailAddress], which accepts values such as "mario@localhost":
    /// requires a domain with at least one dot and no whitespace.
    /// </summary>
    public const string Email = @"^[^@\s]+@[^@\s]+\.[^@\s]{2,}$";
}
