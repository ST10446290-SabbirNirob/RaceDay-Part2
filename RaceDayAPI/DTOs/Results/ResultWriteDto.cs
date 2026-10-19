using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.DTOs.Results;

// Sent when an Organiser captures a new result.
public class ResultWriteDto
{
    [Range(1, int.MaxValue)]
    public int EnrolmentID { get; set; }

    public TimeSpan FinishTime { get; set; }

    [Range(1, int.MaxValue)]
    public int? OverallPosition { get; set; }

    [Range(1, int.MaxValue)]
    public int? CategoryPosition { get; set; }
}
