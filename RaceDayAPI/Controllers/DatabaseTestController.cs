
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;

namespace RaceDayAPI.Controllers
{
    // ============================================================
    // DATABASE TEST CONTROLLER
    // ============================================================
    // This controller checks whether our API can connect
    // to the existing RaceDay SQL Server database.
    //
    // It only READS information.
    // It does not add, update or delete any records.
    // ============================================================

    [ApiController]
    [Route("api/database-test")]
    public class DatabaseTestController : ControllerBase
    {
        // Our Entity Framework database context.
        private readonly RaceDayDbContext _context;

        // Constructor receives the database context.
        public DatabaseTestController(RaceDayDbContext context)
        {
            _context = context;
        }

        // ========================================================
        // GET: api/database-test
        // ========================================================

        [HttpGet]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                // Check whether SQL Server can be reached.
                bool connected = await _context.Database
                    .CanConnectAsync();

                if (!connected)
                {
                    return StatusCode(503, new
                    {
                        Message = "Could not connect to RaceDay database."
                    });
                }

                // Read the number of records in each table.
                int users = await _context.Users.CountAsync();
                int events = await _context.Events.CountAsync();
                int categories = await _context.Categories.CountAsync();
                int routes = await _context.Routes.CountAsync();
                int enrolments = await _context.Enrolments.CountAsync();
                int results = await _context.Results.CountAsync();

                // Return the information as JSON.
                return Ok(new
                {
                    Message = "RaceDay database connection successful!",
                    Database = "RaceDay",
                    Connected = true,

                    RecordCounts = new
                    {
                        Users = users,
                        Events = events,
                        Categories = categories,
                        Routes = routes,
                        Enrolments = enrolments,
                        Results = results
                    }
                });
            }
            catch (Exception)
            {
                // Return a safe error message without exposing
                // private database connection details.
                return StatusCode(503, new
                {
                    Message = "Database connection or query failed. Check the application logs."
                });
            }
        }
    }
}
