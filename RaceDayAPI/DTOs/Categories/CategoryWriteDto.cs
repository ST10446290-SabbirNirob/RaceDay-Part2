using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.DTOs.Categories;

public class CategoryWriteDto
{
    [Required, StringLength(100)]
    public string CategoryName { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "9999.99")]
    public decimal DistanceKm { get; set; }

    [Range(typeof(decimal), "0", "99999999.99")]
    public decimal EntryFee { get; set; }

    [Range(1, 1000000)]
    public int MaxParticipants { get; set; }
}
