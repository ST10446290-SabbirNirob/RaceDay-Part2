
namespace RaceDayAPI.DTOs.Auth
{
    // ============================================================
    // AUTHENTICATION RESPONSE DTO
    // ============================================================
    // This class defines the information that RaceDay sends
    // back after a successful registration or login.
    //
    // It returns basic account information.
    //
    // IMPORTANT:
    // We never return the user's password or password hash.
    // ============================================================

    public class AuthResponseDto
    {
        // ========================================================
        // RESPONSE MESSAGE
        // ========================================================

        // A message explaining what happened.
        //
        // Examples:
        // Registration successful.
        // Login successful.
        public string Message { get; set; } = string.Empty;


        // ========================================================
        // USER INFORMATION
        // ========================================================

        // Unique ID of the logged-in user.
        public int UserID { get; set; }


        // User's first name.
        public string FirstName { get; set; } = string.Empty;


        // User's last name.
        public string LastName { get; set; } = string.Empty;


        // User's email address.
        public string Email { get; set; } = string.Empty;


        // User's role.
        //
        // Organiser or Participant.
        public string Role { get; set; } = string.Empty;
    }
}
