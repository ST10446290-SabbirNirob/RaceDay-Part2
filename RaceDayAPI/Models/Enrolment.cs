
using System;

namespace RaceDayAPI.Models
{
    // ============================================================
    // ENROLMENT MODEL
    // ============================================================
    // This class represents the Enrolment table in our RaceDay
    // database.
    //
    // An Enrolment is created when a Participant enters an Event
    // and selects a Category.
    //
    // Each enrolment connects:
    // 1. A Participant
    // 2. An Event
    // 3. A Category
    //
    // It also stores the enrolment date, status, and race number.
    // ============================================================

    public class Enrolment
    {
        // ========================================================
        // PRIMARY KEY
        // ========================================================

        // Unique ID for each enrolment.
        // SQL: INT IDENTITY(1,1) PRIMARY KEY
        public int EnrolmentID { get; set; }


        // ========================================================
        // FOREIGN KEYS
        // ========================================================

        // Identifies the Participant entering the event.
        // Connects to User.UserID.
        // SQL: INT NOT NULL
        public int ParticipantID { get; set; }


        // Identifies the Event being entered.
        // Connects to Event.EventID.
        // SQL: INT NOT NULL
        public int EventID { get; set; }


        // Identifies the selected Category.
        // Connects to Category.CategoryID.
        // SQL: INT NOT NULL
        public int CategoryID { get; set; }


        // ========================================================
        // ENROLMENT INFORMATION
        // ========================================================

        // Date and time the Participant entered the event.
        // SQL: DATETIME NOT NULL DEFAULT GETDATE()
        public DateTime EnrolmentDate { get; set; }


        // Current enrolment status.
        // Examples: Pending, Confirmed, Cancelled
        // SQL: VARCHAR(20) NOT NULL
        public string Status { get; set; } = string.Empty;


        // Unique race number assigned to this enrolment.
        // Examples: CT001, DBN001, JHB001
        // SQL: VARCHAR(20) NOT NULL UNIQUE
        public string RaceNumber { get; set; } = string.Empty;
    }
}
