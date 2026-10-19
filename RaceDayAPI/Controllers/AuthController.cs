
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using RaceDayAPI.Data;
using RaceDayAPI.DTOs.Auth;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers
{
    // ============================================================
    // RACEDAY AUTHENTICATION CONTROLLER
    // ============================================================
    // This controller handles:
    //
    // 1. User registration
    // 2. User login
    // 3. User logout
    // 4. Checking the current login session
    //
    // Passwords are securely hashed before storage.
    // Login sessions store the UserID and Role.
    // ============================================================

    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        // ========================================================
        // DATABASE AND PASSWORD HASHER
        // ========================================================

        private readonly RaceDayDbContext _context;

        private readonly IPasswordHasher<User> _passwordHasher;


        // ========================================================
        // CONSTRUCTOR
        // ========================================================

        public AuthController(
            RaceDayDbContext context,
            IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }


        // ========================================================
        // REGISTER USER
        // POST: api/auth/register
        // ========================================================

        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponseDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> Register(
            [FromBody] RegisterDto request)
        {
            // Clean up user information.
            string firstName = request.FirstName.Trim();
            string lastName = request.LastName.Trim();

            string email = request.Email.Trim().ToLowerInvariant();

            string requestedRole = request.Role.Trim();

            // Make sure names are not empty.
            if (string.IsNullOrWhiteSpace(firstName) ||
                string.IsNullOrWhiteSpace(lastName))
            {
                return BadRequest(new
                {
                    Message = "First name and last name are required."
                });
            }

            // Check the role selected during registration.
            string role;

            if (requestedRole.Equals(
                "Organiser",
                StringComparison.OrdinalIgnoreCase))
            {
                role = "Organiser";
            }
            else if (requestedRole.Equals(
                "Participant",
                StringComparison.OrdinalIgnoreCase))
            {
                role = "Participant";
            }
            else
            {
                return BadRequest(new
                {
                    Message = "Role must be Organiser or Participant."
                });
            }

            // Check whether the email is already registered.
            bool emailExists = await _context.Users
                .AnyAsync(u => u.Email == email);

            if (emailExists)
            {
                return Conflict(new
                {
                    Message = "An account with this email already exists."
                });
            }

            // Create a new user.
            User user = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Role = role
            };

            // Convert the password into a secure hash.
            // The original password is never saved.
            user.PasswordHash =
                _passwordHasher.HashPassword(
                    user,
                    request.Password);

            // Add the new user to the database.
            _context.Users.Add(user);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is SqlException sqlEx &&
                      (sqlEx.Number == 2601 ||
                       sqlEx.Number == 2627))
            {
                // Handles duplicate email attempts when
                // two registrations happen at the same time.
                return Conflict(new
                {
                    Message = "An account with this email already exists."
                });
            }

            // Prepare a safe response.
            // Never include the password or password hash.
            AuthResponseDto response = new AuthResponseDto
            {
                Message = "Registration successful.",
                UserID = user.UserID,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role
            };

            return StatusCode(201, response);
        }


        // ========================================================
        // LOGIN USER
        // POST: api/auth/login
        // ========================================================

        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Login(
            [FromBody] LoginDto request)
        {
            string email = request.Email.Trim().ToLowerInvariant();

            // Find the registered user.
            User? user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            // Reject unknown email addresses.
            if (user == null)
            {
                return Unauthorized(new
                {
                    Message = "Invalid email or password."
                });
            }

            PasswordVerificationResult passwordResult;

            try
            {
                // Compare the entered password with
                // the stored password hash.
                passwordResult =
                    _passwordHasher.VerifyHashedPassword(
                        user,
                        user.PasswordHash,
                        request.Password);
            }
            catch (FormatException)
            {
                // Handles old sample records that do not
                // contain valid password hashes.
                return Unauthorized(new
                {
                    Message = "Invalid email or password."
                });
            }

            // Reject incorrect passwords.
            if (passwordResult ==
                PasswordVerificationResult.Failed)
            {
                return Unauthorized(new
                {
                    Message = "Invalid email or password."
                });
            }

            // Update older supported password hashes
            // when the hashing system recommends it.
            if (passwordResult ==
                PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.PasswordHash =
                    _passwordHasher.HashPassword(
                        user,
                        request.Password);

                await _context.SaveChangesAsync();
            }

            // Clear any previous session information.
            HttpContext.Session.Clear();

            // Store the authenticated user's information
            // in the server-side session.
            HttpContext.Session.SetInt32(
                "UserID",
                user.UserID);

            HttpContext.Session.SetString(
                "Role",
                user.Role);

            // Return basic user information.
            AuthResponseDto response = new AuthResponseDto
            {
                Message = "Login successful.",
                UserID = user.UserID,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role
            };

            return Ok(response);
        }


        // ========================================================
        // CURRENT USER
        // GET: api/auth/me
        // ========================================================

        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            // Read the UserID from the session.
            int? userId = HttpContext.Session
                .GetInt32("UserID");

            // No session means the user is not logged in.
            if (userId == null)
            {
                return Unauthorized(new
                {
                    Message = "You must log in first."
                });
            }

            // Find the user in the database.
            User? user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    u => u.UserID == userId.Value);

            if (user == null)
            {
                HttpContext.Session.Clear();

                return Unauthorized(new
                {
                    Message = "Your session is no longer valid."
                });
            }

            return Ok(new AuthResponseDto
            {
                Message = "User session is active.",
                UserID = user.UserID,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role
            });
        }


        // ========================================================
        // LOGOUT USER
        // POST: api/auth/logout
        // ========================================================

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // Clear all information stored in the session.
            HttpContext.Session.Clear();

            return Ok(new
            {
                Message = "Logout successful."
            });
        }
    }
}
