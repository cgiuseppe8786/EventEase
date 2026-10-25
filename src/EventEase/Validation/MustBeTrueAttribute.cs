using System.ComponentModel.DataAnnotations;

namespace EventEase.Validation;

/// <summary>
/// Validates that a checkbox has been ticked. [Required] cannot be used for this,
/// because a bool is never null.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class MustBeTrueAttribute : ValidationAttribute
{
    public override bool IsValid(object? value) => value is true;
}
