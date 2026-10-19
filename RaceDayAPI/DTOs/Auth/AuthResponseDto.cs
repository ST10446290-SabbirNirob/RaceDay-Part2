namespace RaceDayAPI.DTOs.Auth;

// Never expose PasswordHash in an API response.
public class AuthResponseDto
{
    public string Message { get; set; } = string.Empty;
    public int UserID { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
