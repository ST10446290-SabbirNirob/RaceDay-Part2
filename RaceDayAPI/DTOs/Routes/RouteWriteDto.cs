using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.DTOs.Routes;

public class RouteWriteDto
{
    [Required, StringLength(100)]
    public string RouteName { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "9999.99")]
    public decimal DistanceKm { get; set; }

    [Required, StringLength(150)]
    public string StartLocation { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string FinishLocation { get; set; } = string.Empty;

    [StringLength(500)]
    public string? RouteInfo { get; set; }
}
