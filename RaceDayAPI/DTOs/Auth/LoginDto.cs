using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.DTOs.Auth;

public class LoginDto
{
    [Required, EmailAddress, StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
