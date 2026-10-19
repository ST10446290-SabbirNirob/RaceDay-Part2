
using System;

namespace RaceDayAPI.DTOs.Users
{
    // ============================================================
    // USER PROFILE DTO
    // ============================================================
    // This class defines what information a logged-in user
    // can see when viewing their RaceDay profile.
    //
    // Both Organisers and Participants can view their profiles.
    //
    // IMPORTANT:
    // Passwords and password hashes are never returned.
    // ============================================================

    public class UserProfileDto
    {
        // Unique ID of the user.
        public int UserID { get; set; }

        // User's first name.
        public string FirstName { get; set; } = string.Empty;

        // User's last name.
        public string LastName { get; set; } = string.Empty;

        // Registered email address.
        public string Email { get; set; } = string.Empty;

        // Account role: Organiser or Participant.
        public string Role { get; set; } = string.Empty;

        // Date when the account was created.
        public DateTime CreatedAt { get; set; }
    }
}
