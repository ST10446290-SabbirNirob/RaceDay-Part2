using System.Globalization;
using System.Security.Claims;

namespace RaceDayAPI.Security;

public static class CurrentUserExtensions
{
    // Only call this from [Authorize] actions.
    // Never trust a UserID supplied in the request body for identity.
    public static int GetRaceDayUserId(this ClaimsPrincipal user)
    {
        string? value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(value, NumberStyles.None,
            CultureInfo.InvariantCulture, out int id))
        {
            throw new UnauthorizedAccessException("Missing authenticated user ID.");
        }
        return id;
    }
}
