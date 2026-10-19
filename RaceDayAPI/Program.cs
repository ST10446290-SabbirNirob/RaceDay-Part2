
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;

// ============================================================
// RACEDAY API - PROGRAM.CS
// ============================================================
// This file starts our RaceDay API.
//
// It configures:
// 1. API Controllers
// 2. Swagger / OpenAPI
// 3. Entity Framework Core
// 4. SQL Server database connection
// 5. HTTP request handling
// ============================================================


// ============================================================
// STEP 1 - CREATE APPLICATION BUILDER
// ============================================================

var builder = WebApplication.CreateBuilder(args);


// ============================================================
// STEP 2 - REGISTER API CONTROLLERS
// ============================================================

// Allows RaceDay to use API Controllers.
builder.Services.AddControllers();


// ============================================================
// STEP 3 - CONFIGURE SWAGGER / OPENAPI
// ============================================================

// Allows Swagger to discover our API endpoints.
builder.Services.AddEndpointsApiExplorer();

// Adds Swagger documentation and testing tools.
builder.Services.AddSwaggerGen();


// ============================================================
// STEP 4 - GET SQL SERVER CONNECTION STRING
// ============================================================

// Reads the RaceDayConnection setting from appsettings.json.
var connectionString = builder.Configuration
    .GetConnectionString("RaceDayConnection");

// Make sure the connection string exists.
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "RaceDayConnection was not found in appsettings.json.");
}


// ============================================================
// STEP 5 - CONFIGURE ENTITY FRAMEWORK CORE
// ============================================================

// Register our RaceDayDbContext.
//
// Entity Framework Core will use SQL Server to access our
// existing RaceDay database.
//
// This does not create or delete database tables.
builder.Services.AddDbContext<RaceDayDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});


// ============================================================
// STEP 6 - BUILD THE APPLICATION
// ============================================================

var app = builder.Build();


// ============================================================
// STEP 7 - CONFIGURE SWAGGER
// ============================================================

// Swagger is enabled when running in Development mode.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// ============================================================
// STEP 8 - CONFIGURE HTTP REQUEST PIPELINE
// ============================================================

// Redirect HTTP requests to HTTPS.
app.UseHttpsRedirection();

// Prepare authorization support.
//
// Authentication and role-based authorization will be
// configured later when we build the login system.
app.UseAuthorization();


// ============================================================
// STEP 9 - MAP API CONTROLLERS
// ============================================================

// Allows requests to reach our API Controllers.
app.MapControllers();


// ============================================================
// STEP 10 - RUN RACEDAY API
// ============================================================

app.Run();
