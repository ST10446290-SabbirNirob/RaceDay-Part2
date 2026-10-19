using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs.Categories;
using RaceDayAPI.Models;
using RaceDayAPI.Security;

namespace RaceDayAPI.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    public CategoriesController(RaceDayDbContext db) => _db = db;

    // GET /api/events/5/categories
    [AllowAnonymous]
    [HttpGet("/api/events/{eventId:int}/categories")]
    public async Task<IActionResult> ForEvent(int eventId)
    {
        if (!await _db.Events.AnyAsync(e => e.EventID == eventId)) return NotFound();
        return Ok(await _db.Categories.AsNoTracking()
            .Where(c => c.EventID == eventId).OrderBy(c => c.DistanceKm).ToListAsync());
    }

    // GET /api/categories/3
    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> ById(int id)
    {
        var category = await _db.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.CategoryID == id);
        return category == null ? NotFound() : Ok(category);
    }

    // POST /api/events/5/categories
    [Authorize(Roles = "Organiser")]
    [HttpPost("/api/events/{eventId:int}/categories")]
    public async Task<IActionResult> Create(int eventId, CategoryWriteDto request)
    {
        var ev = await _db.Events.AsNoTracking().FirstOrDefaultAsync(e => e.EventID == eventId);
        if (ev == null) return NotFound();
        if (ev.OrganiserID != User.GetRaceDayUserId()) return Forbid();
        if (string.IsNullOrWhiteSpace(request.CategoryName)) return BadRequest();

        var item = new Category
        {
            EventID = eventId,
            CategoryName = request.CategoryName.Trim(),
            DistanceKm = request.DistanceKm,
            EntryFee = request.EntryFee,
            MaxParticipants = request.MaxParticipants
        };
        _db.Categories.Add(item);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(ById), new { id = item.CategoryID }, item);
    }

    // PUT /api/categories/3
    [Authorize(Roles = "Organiser")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CategoryWriteDto request)
    {
        var item = await _db.Categories.FirstOrDefaultAsync(c => c.CategoryID == id);
        if (item == null) return NotFound();
        if (!await OwnsEvent(item.EventID)) return Forbid();
        if (string.IsNullOrWhiteSpace(request.CategoryName)) return BadRequest();
        int enrolled = await _db.Enrolments.CountAsync(e => e.CategoryID == id && e.Status != "Cancelled");
        if (request.MaxParticipants < enrolled)
            return Conflict(new { Message = "Maximum capacity cannot be below existing enrolment count." });

        item.CategoryName = request.CategoryName.Trim();
        item.DistanceKm = request.DistanceKm;
        item.EntryFee = request.EntryFee;
        item.MaxParticipants = request.MaxParticipants;
        await _db.SaveChangesAsync();
        return Ok(item);
    }

    // DELETE /api/categories/3
    [Authorize(Roles = "Organiser")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.Categories.FirstOrDefaultAsync(c => c.CategoryID == id);
        if (item == null) return NotFound();
        if (!await OwnsEvent(item.EventID)) return Forbid();
        if (await _db.Enrolments.AnyAsync(e => e.CategoryID == id))
            return Conflict(new { Message = "Category has enrolments and cannot be deleted." });
        _db.Categories.Remove(item);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private async Task<bool> OwnsEvent(int eventId)
    {
        int userId = User.GetRaceDayUserId();
        return await _db.Events.AnyAsync(e => e.EventID == eventId && e.OrganiserID == userId);
    }
}
