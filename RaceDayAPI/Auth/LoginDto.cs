
using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.DTOs.Auth
{
    // ============================================================
    // LOGIN DTO
    // ============================================================
    // This class stores the information entered by a user
    // when logging into the RaceDay application.
    //
    // Users must provide:
    // 1. Email address
    // 2. Password
    //
    // The API will later check this information against
    // the registered users in our SQL Server database.
    // ============================================================

    public class LoginDto
    {
        // ========================================================
        // EMAIL ADDRESS
        // ========================================================

        // The email address used during registration.
        //
        // Required means the user cannot leave it empty.
        // EmailAddress checks that the format is valid.
        // StringLength matches our database limit.

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;


        // ========================================================
        // PASSWORD
        // ========================================================

        // The password entered by the user.
        //
        // The password is required.
        //
        // Later, our authentication system will compare
        // this password with the securely stored password hash.
        //
        // We must never store or return the original password.

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = string.Empty;
    }
}
