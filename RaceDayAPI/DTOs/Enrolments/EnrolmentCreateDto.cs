using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.DTOs.Enrolments;

public class EnrolmentCreateDto
{
    [Range(1, int.MaxValue)]
    public int EventID { get; set; }

    [Range(1, int.MaxValue)]
    public int CategoryID { get; set; }
}
