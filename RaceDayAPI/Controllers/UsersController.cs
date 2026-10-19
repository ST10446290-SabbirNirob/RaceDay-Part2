using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs.Users;
using RaceDayAPI.Security;

namespace RaceDayAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    public UsersController(RaceDayDbContext db) => _db = db;

    // GET /api/users/profile - always uses the logged-in user's ID.
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        int id = User.GetRaceDayUserId();
        var data = await _db.Users.AsNoTracking()
            .Where(u => u.UserID == id)
            .Select(u => new UserProfileDto
            {
                UserID = u.UserID,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                Role = u.Role,
                CreatedAt = u.CreatedAt
            }).FirstOrDefaultAsync();
        return data == null ? NotFound() : Ok(data);
    }

    // PUT /api/users/profile - cannot change a UserID, Role or PasswordHash.
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(UpdateProfileDto request)
    {
        int id = User.GetRaceDayUserId();
        string first = request.FirstName.Trim();
        string last = request.LastName.Trim();
        string email = request.Email.Trim().ToLowerInvariant();
        if (first.Length == 0 || last.Length == 0 || email.Length == 0 ||
            first.Length > 50 || last.Length > 50 || email.Length > 100)
            return BadRequest(new { Message = "Enter valid profile details." });

        if (await _db.Users.AnyAsync(u => u.Email == email && u.UserID != id))
            return Conflict(new { Message = "That email address is already in use." });

        // Part 1 defines Email as a SQL UNIQUE constraint. In RaceDayDbContext,
        // HasAlternateKey maps that existing constraint exactly, but EF does not
        // let tracked alternate keys change. Parameterized ExecuteSql updates it safely.
        try
        {
            int changed = await _db.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE dbo.[User] SET FirstName={first}, LastName={last}, Email={email} WHERE UserID={id}");
            if (changed == 0) return NotFound();
        }
        catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
        {
            return Conflict(new { Message = "That email address is already in use." });
        }

        return await GetProfile();
    }
}
