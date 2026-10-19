
using System;

namespace RaceDayAPI.Models
{
    // ============================================================
    // EVENT MODEL
    // ============================================================
    // This class represents the Event table in our RaceDay database.
    //
    // Organisers can create, update and delete events.
    // Participants can view available events.
    //
    // Each event belongs to one Organiser.
    // ============================================================

    public class Event
    {
        // ========================================================
        // PRIMARY KEY
        // ========================================================

        // Unique ID for each event.
        // SQL: INT IDENTITY(1,1) PRIMARY KEY
        public int EventID { get; set; }


        // ========================================================
        // FOREIGN KEY
        // ========================================================

        // Identifies the Organiser who created the event.
        // Connects to User.UserID.
        // SQL: INT NOT NULL
        public int OrganiserID { get; set; }


        // ========================================================
        // EVENT INFORMATION
        // ========================================================

        // Name of the event.
        // Example: Cape Town City Run
        // SQL: VARCHAR(100) NOT NULL
        public string EventName { get; set; } = string.Empty;


        // Additional information about the event.
        // This field is optional.
        // SQL: VARCHAR(500) NULL
        public string? Description { get; set; }


        // Date and time when the event takes place.
        // SQL: DATETIME NOT NULL
        public DateTime EventDate { get; set; }


        // Location where the event takes place.
        // Example: Cape Town
        // SQL: VARCHAR(150) NOT NULL
        public string Location { get; set; } = string.Empty;


        // Type of event.
        // Examples: Run, Walk, Cycle
        // SQL: VARCHAR(30) NOT NULL
        public string EventType { get; set; } = string.Empty;


        // Last date and time participants can register.
        // SQL: DATETIME NOT NULL
        public DateTime RegistrationDeadline { get; set; }
    }
}
