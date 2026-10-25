using System.ComponentModel.DataAnnotations;
using EventEase.Validation;

namespace EventEase.Models;

/// <summary>
/// An event managed by EventEase. Validation attributes are used both by the
/// edit form inside <c>EventCard</c> and by the unit tests.
/// </summary>
public sealed class EventItem
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Il nome dell'evento è obbligatorio.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Il nome deve contenere tra {2} e {1} caratteri.")]
    public string Name { get; set; } = string.Empty;

    [FutureDate(ErrorMessage = "La data dell'evento deve essere nel futuro.")]
    public DateTime Date { get; set; }

    [Required(ErrorMessage = "Il luogo è obbligatorio.")]
    [StringLength(120, MinimumLength = 2, ErrorMessage = "Il luogo deve contenere tra {2} e {1} caratteri.")]
    public string Location { get; set; } = string.Empty;

    [Required(ErrorMessage = "La categoria è obbligatoria.")]
    public string Category { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "La descrizione non può superare {1} caratteri.")]
    public string? Description { get; set; }

    [Range(1, 5000, ErrorMessage = "La capienza deve essere compresa tra {1} e {2} posti.")]
    public int Capacity { get; set; }

    /// <summary>
    /// Returns a detached copy, so that edits can be cancelled without touching the original.
    /// </summary>
    public EventItem Clone() => (EventItem)MemberwiseClone();
}
