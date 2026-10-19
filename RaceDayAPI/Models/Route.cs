
namespace RaceDayAPI.Models
{
    // ============================================================
    // ROUTE MODEL
    // ============================================================
    // This class represents the Route table in our RaceDay
    // database.
    //
    // A route contains information about the path participants
    // will follow during an event.
    //
    // Each route belongs to one Event.
    // One Event can have multiple Routes.
    //
    // Example:
    // Event: Cape Town City Run
    // Route: City Centre 10km Route
    // ============================================================

    public class Route
    {
        // ========================================================
        // PRIMARY KEY
        // ========================================================

        // Unique ID for each route.
        // SQL: INT IDENTITY(1,1) PRIMARY KEY
        public int RouteID { get; set; }


        // ========================================================
        // FOREIGN KEY
        // ========================================================

        // Identifies the Event this route belongs to.
        // Connects to Event.EventID.
        // SQL: INT NOT NULL
        public int EventID { get; set; }


        // ========================================================
        // ROUTE INFORMATION
        // ========================================================

        // Name of the route.
        // Example: City Centre 10km Route
        // SQL: VARCHAR(100) NOT NULL
        public string RouteName { get; set; } = string.Empty;


        // Total route distance in kilometres.
        // Example: 10.00 or 21.10
        // SQL: DECIMAL(6,2) NOT NULL
        public decimal DistanceKm { get; set; }


        // Location where the route begins.
        // Example: Green Point Stadium
        // SQL: VARCHAR(150) NOT NULL
        public string StartLocation { get; set; } = string.Empty;


        // Location where the route finishes.
        // Example: Cape Town City Hall
        // SQL: VARCHAR(150) NOT NULL
        public string FinishLocation { get; set; } = string.Empty;


        // Additional information about the route.
        // This field is optional.
        // SQL: VARCHAR(500) NULL
        public string? RouteInfo { get; set; }
    }
}
