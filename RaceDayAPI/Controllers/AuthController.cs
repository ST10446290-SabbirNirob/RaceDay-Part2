using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs.Auth;
using RaceDayAPI.Models;
using RaceDayAPI.Security;

namespace RaceDayAPI.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    private readonly IPasswordHasher<User> _hasher;

    public AuthController(RaceDayDbContext db, IPasswordHasher<User> hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    // POST /api/auth/register - users may choose Organiser or Participant.
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Register(RegisterDto request)
    {
        string firstName = request.FirstName.Trim();
        string lastName = request.LastName.Trim();
        string email = request.Email.Trim().ToLowerInvariant();
        string role = request.Role.Trim();

        if (firstName.Length == 0 || lastName.Length == 0 || email.Length == 0 ||
            firstName.Length > 50 || lastName.Length > 50 || email.Length > 100)
            return BadRequest(new { Message = "Enter valid names and email." });

        if (role.Equals("Organiser", StringComparison.OrdinalIgnoreCase))
        {
            role = "Organiser";
        }
        else if (role.Equals("Participant", StringComparison.OrdinalIgnoreCase))
            role = "Participant";
        else
            return BadRequest(new { Message = "Role must be Organiser or Participant." });

        if (await _db.Users.AnyAsync(u => u.Email == email))
            return Conflict(new { Message = "An account with this email already exists." });

        var user = new User
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Role = role,
            CreatedAt = DateTime.Now
        };
        user.PasswordHash = _hasher.HashPassword(user, request.Password);
        _db.Users.Add(user);

        try { await _db.SaveChangesAsync(); }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sql &&
            (sql.Number == 2601 || sql.Number == 2627))
        {
            return Conflict(new { Message = "An account with this email already exists." });
        }

        return StatusCode(201, ToResponse("Registration successful.", user));
    }

    // POST /api/auth/login - verify password, then create the server-side session.
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login(LoginDto request)
    {
        string email = request.Email.Trim().ToLowerInvariant();
        User? user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
            return Unauthorized(new { Message = "Invalid email or password." });

        PasswordVerificationResult result;
        try { result = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password); }
        catch (FormatException) { result = PasswordVerificationResult.Failed; }

        // The Part 1 seed accounts contain sample placeholders, not real hashes.
        if (result == PasswordVerificationResult.Failed)
            return Unauthorized(new { Message = "Invalid email or password." });

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _hasher.HashPassword(user, request.Password);
            await _db.SaveChangesAsync();
        }

        HttpContext.Session.Clear();
        HttpContext.Session.SetInt32("UserID", user.UserID);
        HttpContext.Session.SetString("Role", user.Role);
        await HttpContext.Session.CommitAsync();
        return Ok(ToResponse("Login successful.", user));
    }

    // GET /api/auth/me - return safe profile information for the signed-in user.
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        int id = User.GetRaceDayUserId();
        User? user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserID == id);
        return user == null
            ? Unauthorized(new { Message = "Session is no longer valid." })
            : Ok(ToResponse("User session is active.", user));
    }

    // POST /api/auth/logout - needs X-RaceDay-Request header (CSRF protection).
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        HttpContext.Session.Clear();
        await HttpContext.Session.CommitAsync();
        return Ok(new { Message = "Logout successful." });
    }

    private static AuthResponseDto ToResponse(string message, User user) => new()
    {
        Message = message,
        UserID = user.UserID,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Email = user.Email,
        Role = user.Role
    };
}
