using System.ComponentModel.DataAnnotations;
using EventEase.Validation;

namespace EventEase.Models;

/// <summary>
/// Data entered by a participant in the event registration form.
/// </summary>
public sealed class RegistrationModel
{
    private string? _phone;

    [Required(ErrorMessage = "Il nome completo è obbligatorio.")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "Il nome deve contenere tra {2} e {1} caratteri.")]
    [RegularExpression(ValidationPatterns.PersonName, ErrorMessage = "Il nome può contenere solo lettere, spazi, apostrofi e trattini.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "L'email è obbligatoria.")]
    [StringLength(254, ErrorMessage = "L'email non può superare {1} caratteri.")]
    [RegularExpression(ValidationPatterns.Email, ErrorMessage = "Inserisci un indirizzo email valido (es. nome@dominio.it).")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Optional. An emptied input produces "" rather than null, which [Phone] rejects,
    /// so blank values are normalised to null.
    /// </summary>
    [Phone(ErrorMessage = "Inserisci un numero di telefono valido.")]
    [StringLength(20, ErrorMessage = "Il numero di telefono non può superare {1} caratteri.")]
    public string? Phone
    {
        get => _phone;
        set => _phone = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    [MustBeTrue(ErrorMessage = "Devi accettare il trattamento dei dati per completare la registrazione.")]
    public bool AcceptTerms { get; set; }
}
