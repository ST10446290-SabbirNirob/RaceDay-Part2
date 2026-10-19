using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.DTOs.Events;

public class EventWriteDto : IValidatableObject
{
    [Required, StringLength(100)]
    public string EventName { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    public DateTime EventDate { get; set; }

    [Required, StringLength(150)]
    public string Location { get; set; } = string.Empty;

    [Required, StringLength(30)]
    public string EventType { get; set; } = string.Empty;

    [Required]
    public DateTime RegistrationDeadline { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (EventDate == default || RegistrationDeadline == default)
            yield return new ValidationResult("Provide valid event and deadline dates.");
        else if (RegistrationDeadline >= EventDate)
            yield return new ValidationResult("Registration must close before the event starts.");

        if (!new[] { "Running", "Walking", "Cycling" }
            .Contains(EventType?.Trim(), StringComparer.OrdinalIgnoreCase))
            yield return new ValidationResult("EventType must be Running, Walking or Cycling.");
    }
}
