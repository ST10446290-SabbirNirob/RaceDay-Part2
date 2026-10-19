using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs.Routes;
using RaceDayAPI.Models;
using RaceDayAPI.Security;
using RaceRoute = RaceDayAPI.Models.Route;

namespace RaceDayAPI.Controllers;

// Extra endpoints for the Route table, useful when building the Part 3 MVC app.
[ApiController]
[Route("api/routes")]
public class RoutesController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    public RoutesController(RaceDayDbContext db) => _db = db;

    [AllowAnonymous]
    [HttpGet("/api/events/{eventId:int}/routes")]
    public async Task<IActionResult> ForEvent(int eventId)
    {
        if (!await _db.Events.AnyAsync(e => e.EventID == eventId)) return NotFound();
        return Ok(await _db.Routes.AsNoTracking().Where(r => r.EventID == eventId).ToListAsync());
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> ById(int id)
    {
        var route = await _db.Routes.AsNoTracking().FirstOrDefaultAsync(r => r.RouteID == id);
        return route == null ? NotFound() : Ok(route);
    }

    [Authorize(Roles = "Organiser")]
    [HttpPost("/api/events/{eventId:int}/routes")]
    public async Task<IActionResult> Create(int eventId, RouteWriteDto request)
    {
        var ev = await _db.Events.AsNoTracking().FirstOrDefaultAsync(e => e.EventID == eventId);
        if (ev == null) return NotFound();
        if (ev.OrganiserID != User.GetRaceDayUserId()) return Forbid();
        if (!ValidText(request)) return BadRequest();
        var route = new RaceRoute { EventID = eventId };
        Apply(route, request);
        _db.Routes.Add(route);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(ById), new { id = route.RouteID }, route);
    }

    [Authorize(Roles = "Organiser")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, RouteWriteDto request)
    {
        var route = await _db.Routes.FirstOrDefaultAsync(r => r.RouteID == id);
        if (route == null) return NotFound();
        if (!await OwnsEvent(route.EventID)) return Forbid();
        if (!ValidText(request)) return BadRequest();
        Apply(route, request);
        await _db.SaveChangesAsync();
        return Ok(route);
    }

    [Authorize(Roles = "Organiser")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var route = await _db.Routes.FirstOrDefaultAsync(r => r.RouteID == id);
        if (route == null) return NotFound();
        if (!await OwnsEvent(route.EventID)) return Forbid();
        _db.Routes.Remove(route);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private async Task<bool> OwnsEvent(int eventId)
    {
        int id = User.GetRaceDayUserId();
        return await _db.Events.AnyAsync(e => e.EventID == eventId && e.OrganiserID == id);
    }

    private static bool ValidText(RouteWriteDto r) =>
        !string.IsNullOrWhiteSpace(r.RouteName) &&
        !string.IsNullOrWhiteSpace(r.StartLocation) &&
        !string.IsNullOrWhiteSpace(r.FinishLocation);

    private static void Apply(RaceRoute r, RouteWriteDto dto)
    {
        r.RouteName = dto.RouteName.Trim();
        r.DistanceKm = dto.DistanceKm;
        r.StartLocation = dto.StartLocation.Trim();
        r.FinishLocation = dto.FinishLocation.Trim();
        r.RouteInfo = dto.RouteInfo?.Trim();
    }
}
