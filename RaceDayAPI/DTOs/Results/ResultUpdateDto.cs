using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.DTOs.Results;

// EnrolmentID cannot change after a result has been created.
public class ResultUpdateDto
{
    public TimeSpan FinishTime { get; set; }

    [Range(1, int.MaxValue)]
    public int? OverallPosition { get; set; }

    [Range(1, int.MaxValue)]
    public int? CategoryPosition { get; set; }
}
