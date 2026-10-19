
namespace RaceDayAPI.Models
{
    // ============================================================
    // CATEGORY MODEL
    // ============================================================
    // This class represents the Category table in our database.
    //
    // Each Event can have multiple Categories.
    //
    // For example:
    // Cape Town City Run
    // - 5km Run
    // - 10km Run
    // - 21km Half Marathon
    //
    // Organisers create and manage categories.
    // Participants select a category when entering an event.
    // ============================================================

    public class Category
    {
        // ========================================================
        // PRIMARY KEY
        // ========================================================

        // Unique ID for each category.
        // SQL: INT IDENTITY(1,1) PRIMARY KEY
        public int CategoryID { get; set; }


        // ========================================================
        // FOREIGN KEY
        // ========================================================

        // Identifies the Event this category belongs to.
        // Connects to Event.EventID.
        // SQL: INT NOT NULL
        public int EventID { get; set; }


        // ========================================================
        // CATEGORY INFORMATION
        // ========================================================

        // Name of the category.
        // Examples: 5km Run, 10km Run, Senior
        // SQL: VARCHAR(100) NOT NULL
        public string CategoryName { get; set; } = string.Empty;


        // Distance of the category in kilometres.
        // Example: 5.00, 10.00, 21.10
        // SQL: DECIMAL(6,2) NOT NULL
        public decimal DistanceKm { get; set; }


        // Price participants must pay to enter.
        // Example: 150.00
        // SQL: DECIMAL(10,2) NOT NULL
        public decimal EntryFee { get; set; }


        // Maximum number of participants allowed.
        // Example: 500
        // SQL: INT NOT NULL
        public int MaxParticipants { get; set; }
    }
}
