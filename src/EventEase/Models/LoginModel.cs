using System.ComponentModel.DataAnnotations;
using EventEase.Validation;

namespace EventEase.Models;

/// <summary>
/// Data required to start a user session.
/// </summary>
public sealed class LoginModel
{
    [Required(ErrorMessage = "Il nome è obbligatorio.")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "Il nome deve contenere tra {2} e {1} caratteri.")]
    [RegularExpression(ValidationPatterns.PersonName, ErrorMessage = "Il nome può contenere solo lettere, spazi, apostrofi e trattini.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "L'email è obbligatoria.")]
    [StringLength(254, ErrorMessage = "L'email non può superare {1} caratteri.")]
    [RegularExpression(ValidationPatterns.Email, ErrorMessage = "Inserisci un indirizzo email valido (es. nome@dominio.it).")]
    public string Email { get; set; } = string.Empty;
}
