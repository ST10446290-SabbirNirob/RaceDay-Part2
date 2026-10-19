using Microsoft.AspNetCore.Http;

namespace RaceDayAPI.Security;

// Blocks basic cross-site form requests that try to use a logged-in session.
// For changing requests on an authenticated session, clients must send the
// custom header X-RaceDay-Request: 1. Browsers cannot send this custom header
// cross-origin without CORS permission (and we do not enable credentialed CORS).
// Secure + HttpOnly + SameSite.Strict are configured for the session cookie.
public sealed class SessionRequestProtectionMiddleware
{
    private readonly RequestDelegate _next;
    public SessionRequestProtectionMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        bool changesData = HttpMethods.IsPost(context.Request.Method)
            || HttpMethods.IsPut(context.Request.Method)
            || HttpMethods.IsPatch(context.Request.Method)
            || HttpMethods.IsDelete(context.Request.Method);

        // Registration and login use credentials supplied in the request,
        // not an existing login session. They are excluded from this header rule.
        bool publicAuth = context.Request.Path.Equals("/api/auth/login", StringComparison.OrdinalIgnoreCase)
            || context.Request.Path.Equals("/api/auth/register", StringComparison.OrdinalIgnoreCase);

        if (changesData && !publicAuth && context.User.Identity?.IsAuthenticated == true &&
            context.Request.Headers["X-RaceDay-Request"] != "1")
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                Message = "This request requires the X-RaceDay-Request header with value 1."
            });
            return;
        }
        await _next(context);
    }
}
