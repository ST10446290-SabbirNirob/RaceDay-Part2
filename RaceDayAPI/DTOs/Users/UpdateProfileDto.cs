
using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.DTOs.Users
{
    // ============================================================
    // UPDATE PROFILE DTO
    // ============================================================
    // This class defines the information a logged-in user
    // can update in their RaceDay account.
    //
    // Users can update:
    // 1. First name
    // 2. Last name
    // 3. Email address
    //
    // Users cannot change their UserID or Role through
    // this request.
    // ============================================================

    public class UpdateProfileDto
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
    }
}
