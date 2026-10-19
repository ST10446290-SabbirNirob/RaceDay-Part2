
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.Models;

// ============================================================
// RACEDAY API - PROGRAM.CS
// ============================================================
// This file starts our RaceDay API and configures:
// 1. API Controllers
// 2. Swagger
// 3. SQL Server and Entity Framework Core
// 4. Secure password hashing
// 5. User login sessions
// 6. HTTP request handling
// ============================================================


// ============================================================
// STEP 1 - CREATE APPLICATION BUILDER
// ============================================================

var builder = WebApplication.CreateBuilder(args);


// ============================================================
// STEP 2 - REGISTER API CONTROLLERS
// ============================================================

builder.Services.AddControllers();


// ============================================================
// STEP 3 - CONFIGURE SWAGGER
// ============================================================

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// ============================================================
// STEP 4 - GET DATABASE CONNECTION STRING
// ============================================================

var connectionString = builder.Configuration
    .GetConnectionString("RaceDayConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "RaceDayConnection was not found in appsettings.json.");
}


// ============================================================
// STEP 5 - CONFIGURE ENTITY FRAMEWORK CORE
// ============================================================

builder.Services.AddDbContext<RaceDayDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});


// ============================================================
// STEP 6 - CONFIGURE PASSWORD HASHING
// ============================================================

// PasswordHasher securely hashes passwords before we save
// them to SQL Server.
//
// It also verifies passwords when someone logs in.
//
// We do not store plain-text passwords.

builder.Services.AddScoped<
    IPasswordHasher<User>,
    PasswordHasher<User>>();


// ============================================================
// STEP 7 - CONFIGURE SESSION SERVICES
// ============================================================

// Sessions need a cache to store session information.
// This uses memory while the application is running.

builder.Services.AddDistributedMemoryCache();

// Configure login sessions.

builder.Services.AddSession(options =>
{
    // End the session after 30 minutes of inactivity.
    options.IdleTimeout = TimeSpan.FromMinutes(30);

    // JavaScript cannot read the session cookie.
    options.Cookie.HttpOnly = true;

    // Only send the session cookie over HTTPS.
    options.Cookie.SecurePolicy =
        CookieSecurePolicy.Always;

    // Helps reduce some cross-site request risks.
    options.Cookie.SameSite =
        SameSiteMode.Lax;

    // Allow the session cookie to be used by the API.
    options.Cookie.IsEssential = true;

    // Give our session cookie a clear name.
    options.Cookie.Name = ".RaceDay.Session";
});


// ============================================================
// STEP 8 - BUILD THE APPLICATION
// ============================================================

var app = builder.Build();


// ============================================================
// STEP 9 - ENABLE SWAGGER
// ============================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// ============================================================
// STEP 10 - CONFIGURE HTTP PIPELINE
// ============================================================

// Redirect HTTP requests to HTTPS.
app.UseHttpsRedirection();

// Enable routing.
app.UseRouting();

// Enable session support.
//
// This must run before controllers access HttpContext.Session.
app.UseSession();

// Authorization policies and authentication checks
// will be configured as we add protected endpoints.
app.UseAuthorization();

// Map API Controllers.
app.MapControllers();


// ============================================================
// STEP 11 - RUN THE RACEDAY API
// ============================================================

app.Run();
