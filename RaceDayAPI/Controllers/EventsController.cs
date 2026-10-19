using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs.Events;
using RaceDayAPI.Models;
using RaceDayAPI.Security;

namespace RaceDayAPI.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    public EventsController(RaceDayDbContext db) => _db = db;

    // GET /api/events - public list.
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _db.Events.AsNoTracking().OrderBy(e => e.EventDate).ToListAsync());

    // GET /api/events/mine - Organiser's own events.
    [Authorize(Roles = "Organiser")]
    [HttpGet("mine")]
    public async Task<IActionResult> Mine()
    {
        int id = User.GetRaceDayUserId();
        return Ok(await _db.Events.AsNoTracking()
            .Where(e => e.OrganiserID == id).OrderBy(e => e.EventDate).ToListAsync());
    }

    // GET /api/events/5 - includes routes and category distances.
    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var ev = await _db.Events.AsNoTracking().FirstOrDefaultAsync(e => e.EventID == id);
        if (ev == null) return NotFound();
        return Ok(new
        {
            Event = ev,
            Categories = await _db.Categories.AsNoTracking().Where(c => c.EventID == id).ToListAsync(),
            Routes = await _db.Routes.AsNoTracking().Where(r => r.EventID == id).ToListAsync()
        });
    }

    // POST /api/events - only an Organiser may create events.
    [Authorize(Roles = "Organiser")]
    [HttpPost]
    public async Task<IActionResult> Create(EventWriteDto request)
    {
        if (!ValidText(request)) return BadRequest(new { Message = "Event details are required." });
        var ev = new Event
        {
            OrganiserID = User.GetRaceDayUserId(), // never trust an organiser ID from JSON
            EventName = request.EventName.Trim(),
            Description = request.Description?.Trim(),
            EventDate = request.EventDate,
            Location = request.Location.Trim(),
            EventType = NormalizeType(request.EventType),
            RegistrationDeadline = request.RegistrationDeadline
        };
        _db.Events.Add(ev);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = ev.EventID }, ev);
    }

    // PUT /api/events/5 - an Organiser can only edit THEIR OWN event.
    [Authorize(Roles = "Organiser")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, EventWriteDto request)
    {
        var ev = await _db.Events.FirstOrDefaultAsync(e => e.EventID == id);
        if (ev == null) return NotFound();
        if (ev.OrganiserID != User.GetRaceDayUserId()) return Forbid();
        if (!ValidText(request)) return BadRequest(new { Message = "Event details are required." });

        ev.EventName = request.EventName.Trim();
        ev.Description = request.Description?.Trim();
        ev.EventDate = request.EventDate;
        ev.Location = request.Location.Trim();
        ev.EventType = NormalizeType(request.EventType);
        ev.RegistrationDeadline = request.RegistrationDeadline;
        await _db.SaveChangesAsync();
        return Ok(ev);
    }

    // DELETE /api/events/5 - no cascading deletes in the original Part 1 SQL.
    [Authorize(Roles = "Organiser")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ev = await _db.Events.FirstOrDefaultAsync(e => e.EventID == id);
        if (ev == null) return NotFound();
        if (ev.OrganiserID != User.GetRaceDayUserId()) return Forbid();

        bool hasRelatedData = await _db.Categories.AnyAsync(c => c.EventID == id)
            || await _db.Routes.AnyAsync(r => r.EventID == id)
            || await _db.Enrolments.AnyAsync(e => e.EventID == id);
        if (hasRelatedData)
            return Conflict(new { Message = "This event has categories, routes or enrolments. Remove dependent records first; enrolment history should be preserved." });

        _db.Events.Remove(ev);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static bool ValidText(EventWriteDto r) =>
        !string.IsNullOrWhiteSpace(r.EventName) && !string.IsNullOrWhiteSpace(r.Location);

    private static string NormalizeType(string input) => input.Trim().ToLowerInvariant() switch
    {
        "run" or "running" => "Running",
        "walk" or "walking" => "Walking",
        "cycle" or "cycling" => "Cycling",
        _ => input.Trim()
    };
}
