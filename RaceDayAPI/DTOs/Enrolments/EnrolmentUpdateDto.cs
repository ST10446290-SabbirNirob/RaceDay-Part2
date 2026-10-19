using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.DTOs.Enrolments;

public class EnrolmentUpdateDto
{
    [Required]
    [RegularExpression("^(Pending|Confirmed|Cancelled)$", ErrorMessage = "Status must be Pending, Confirmed, or Cancelled.")]
    public string Status { get; set; } = string.Empty;
}
