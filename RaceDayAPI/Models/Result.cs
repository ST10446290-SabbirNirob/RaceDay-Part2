
using System;

namespace RaceDayAPI.Models
{
    // ============================================================
    // RESULT MODEL
    // ============================================================
    // This class represents the Result table in our RaceDay
    // database.
    //
    // Organisers record results after an event has finished.
    //
    // Each Result belongs to one Enrolment.
    // Each Enrolment can have a maximum of one Result.
    //
    // Participants can view their own race results.
    // ============================================================

    public class Result
    {
        // ========================================================
        // PRIMARY KEY
        // ========================================================

        // Unique ID for each result.
        // SQL: INT IDENTITY(1,1) PRIMARY KEY
        public int ResultID { get; set; }


        // ========================================================
        // FOREIGN KEY
        // ========================================================

        // Identifies the Enrolment this result belongs to.
        // Connects to Enrolment.EnrolmentID.
        //
        // SQL: INT NOT NULL UNIQUE
        //
        // The UNIQUE constraint ensures that each enrolment
        // can only have one result.
        public int EnrolmentID { get; set; }


        // ========================================================
        // RACE RESULT INFORMATION
        // ========================================================

        // The time taken by the participant to finish.
        //
        // Example:
        // 01:25:30 means 1 hour, 25 minutes and 30 seconds.
        //
        // SQL: TIME NOT NULL
        public TimeSpan FinishTime { get; set; }


        // The participant's finishing position
        // compared to all participants in the event.
        //
        // Example: 15 means 15th place.
        //
        // This field is optional.
        // SQL: INT NULL
        public int? OverallPosition { get; set; }


        // The participant's finishing position
        // within their selected category.
        //
        // Example: 3 means 3rd place in the category.
        //
        // This field is optional.
        // SQL: INT NULL
        public int? CategoryPosition { get; set; }


        // Date and time when the result was recorded.
        //
        // SQL: DATETIME NOT NULL DEFAULT GETDATE()
        public DateTime RecordedAt { get; set; }
    }
}
