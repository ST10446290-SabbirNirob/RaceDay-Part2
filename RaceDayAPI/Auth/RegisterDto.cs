
using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.DTOs.Auth
{
    // ============================================================
    // REGISTER DTO
    // ============================================================
    // This class stores the information sent by a user
    // when creating a new RaceDay account.
    //
    // It validates basic registration information before
    // the API attempts to create a new user.
    // ============================================================

    public class RegisterDto
    {
        // ========================================================
        // FIRST NAME
        // ========================================================

        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;


        // ========================================================
        // LAST NAME
        // ========================================================

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;


        // ========================================================
        // EMAIL ADDRESS
        // ========================================================

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;


        // ========================================================
        // PASSWORD
        // ========================================================

        // This is the password entered during registration.
        //
        // Later, we will securely hash it before storing it
        // in the User table.
        //
        // We NEVER save the original password directly.

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        public string Password { get; set; } = string.Empty;


        // ========================================================
        // USER ROLE
        // ========================================================

        // The assignment supports two roles:
        // Organiser and Participant.
        //
        // We will validate the role again on the server.
        // Organiser registration needs careful handling
        // because it gives the user extra permissions.

        [Required(ErrorMessage = "Role is required.")]
        public string Role { get; set; } = string.Empty;
    }
}
