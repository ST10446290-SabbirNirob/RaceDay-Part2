using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using RaceDayAPI.Authentication;
using RaceDayAPI.Data;
using RaceDayAPI.Models;
using RaceDayAPI.Security;
using RaceDayAPI.Swagger;

var builder = WebApplication.CreateBuilder(args);

// API controllers + Swagger browser testing.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "RaceDay API",
        Version = "v1",
        Description = "RaceDay event management API. Organiser and Participant role permissions."
    });
    options.OperationFilter<RaceDayHeaderOperationFilter>();
    options.OperationFilter<RaceDayDescriptionOperationFilter>();
});

// EF Core: use the ORIGINAL RaceDay database. This doesn't run migrations.
string connectionString = builder.Configuration.GetConnectionString("RaceDayConnection")
    ?? throw new InvalidOperationException("Missing ConnectionStrings:RaceDayConnection.");
builder.Services.AddDbContext<RaceDayDbContext>(options => options.UseSqlServer(connectionString));

// Passwords are securely hashed with ASP.NET Core's built-in Identity hasher.
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// Server-side session; the browser only holds a session ID cookie.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.Name = ".RaceDay.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.IsEssential = true;
});

// Custom authentication reads the session and verifies the current database role.
builder.Services.AddAuthentication("RaceDaySession")
    .AddScheme<AuthenticationSchemeOptions, SessionAuthenticationHandler>(
        "RaceDaySession", _ => { });
builder.Services.AddAuthorization();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();      // session data must exist before the authentication handler
app.UseAuthentication();
app.UseMiddleware<SessionRequestProtectionMiddleware>();
app.UseAuthorization();
app.MapControllers();
app.Run();

// Required by the WebApplicationFactory integration tests.
public partial class Program { }
