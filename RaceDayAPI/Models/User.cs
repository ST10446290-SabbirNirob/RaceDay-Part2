
using System;

namespace RaceDayAPI.Models
{
    // ============================================================
    // USER MODEL
    // ============================================================
    // This class represents the User table in our RaceDay database.
    //
    // A user can be either:
    // 1. Organiser
    // 2. Participant
    //
    // The Role property identifies the user's account type.
    // ============================================================

    public class User
    {
        // Primary Key
        // SQL Server automatically generates a unique UserID.
        public int UserID { get; set; }

        // User's first name.
        // SQL: VARCHAR(50) NOT NULL
        public string FirstName { get; set; } = string.Empty;

        // User's last name.
        // SQL: VARCHAR(50) NOT NULL
        public string LastName { get; set; } = string.Empty;

        // User's email address.
        // SQL: VARCHAR(100) NOT NULL UNIQUE
        public string Email { get; set; } = string.Empty;

        // Stores the hashed password.
        // Never store the original password here.
        // SQL: VARCHAR(255) NOT NULL
        public string PasswordHash { get; set; } = string.Empty;

        // Identifies the account type.
        // Allowed values: Organiser or Participant.
        // SQL: VARCHAR(20) NOT NULL
        public string Role { get; set; } = string.Empty;

        // Date and time the account was created.
        // SQL: DATETIME NOT NULL DEFAULT GETDATE()
        public DateTime CreatedAt { get; set; }
    }
}
