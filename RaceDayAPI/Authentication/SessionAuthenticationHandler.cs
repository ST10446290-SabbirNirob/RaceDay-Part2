using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RaceDayAPI.Data;

namespace RaceDayAPI.Authentication;

// Connects ASP.NET Core [Authorize] to RaceDay's server-side login session.
public class SessionAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly RaceDayDbContext _db;

    public SessionAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        RaceDayDbContext db)
        : base(options, logger, encoder)
    {
        _db = db;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        int? id = Context.Session.GetInt32("UserID");
        string? storedRole = Context.Session.GetString("Role");
        if (id == null || string.IsNullOrWhiteSpace(storedRole))
            return AuthenticateResult.NoResult();

        var user = await _db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserID == id.Value);
        if (user == null || user.Role != storedRole)
        {
            Context.Session.Clear();
            return AuthenticateResult.Fail("Session is no longer valid.");
        }

        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
            new Claim(ClaimTypes.Name, user.FirstName + " " + user.LastName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        }, Scheme.Name);

        return AuthenticateResult.Success(
            new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name));
    }
}
