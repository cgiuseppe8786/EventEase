using System.ComponentModel.DataAnnotations;

namespace EventEase.Validation;

/// <summary>
/// Validates that a <see cref="DateTime"/> is later than the current local time.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class FutureDateAttribute : ValidationAttribute
{
    public override bool IsValid(object? value) => value is DateTime date && date > DateTime.Now;
}
