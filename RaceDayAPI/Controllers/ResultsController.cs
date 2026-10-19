using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs.Results;
using RaceDayAPI.Models;
using RaceDayAPI.Security;

namespace RaceDayAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/results")]
public class ResultsController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    public ResultsController(RaceDayDbContext db) => _db = db;

    // POST /api/results - the event Organiser captures a finishing result.
    [Authorize(Roles = "Organiser")]
    [HttpPost]
    public async Task<IActionResult> Create(ResultWriteDto request)
    {
        if (!ValidFinishTime(request.FinishTime))
            return BadRequest(new { Message = "FinishTime must be greater than zero and less than 24 hours." });

        var entry = await _db.Enrolments.AsNoTracking()
            .FirstOrDefaultAsync(e => e.EnrolmentID == request.EnrolmentID);
        if (entry == null) return NotFound(new { Message = "Enrolment not found." });
        if (!await OwnsEvent(entry.EventID)) return Forbid();
        if (entry.Status == "Cancelled")
            return Conflict(new { Message = "Cannot add results to a cancelled entry." });
        if (await _db.Results.AnyAsync(r => r.EnrolmentID == request.EnrolmentID))
            return Conflict(new { Message = "This enrolment already has a result." });

        // Part 1 seed data includes sample results for future-dated events.
        // We leave those old rows unchanged, but new results require the event to be finished.
        if (await _db.Events.AnyAsync(e => e.EventID == entry.EventID && e.EventDate > DateTime.Now))
            return Conflict(new { Message = "Results can only be captured after the event date." });

        var result = new Result
        {
            EnrolmentID = request.EnrolmentID,
            FinishTime = request.FinishTime,
            OverallPosition = request.OverallPosition,
            CategoryPosition = request.CategoryPosition,
            RecordedAt = DateTime.Now
        };
        _db.Results.Add(result);
        await _db.SaveChangesAsync();
        return StatusCode(201, result);
    }

    // GET /api/results/my - participant sees only their own results.
    [Authorize(Roles = "Participant")]
    [HttpGet("my")]
    public async Task<IActionResult> MyResults()
    {
        int participantId = User.GetRaceDayUserId();
        var rows = await (from result in _db.Results.AsNoTracking()
                          join entry in _db.Enrolments.AsNoTracking() on result.EnrolmentID equals entry.EnrolmentID
                          join ev in _db.Events.AsNoTracking() on entry.EventID equals ev.EventID
                          join category in _db.Categories.AsNoTracking() on entry.CategoryID equals category.CategoryID
                          where entry.ParticipantID == participantId
                          orderby ev.EventDate descending
                          select new
                          {
                              result.ResultID, result.EnrolmentID,
                              ev.EventID, ev.EventName, ev.EventDate,
                              category.CategoryName, result.FinishTime,
                              result.OverallPosition, result.CategoryPosition,
                              result.RecordedAt
                          }).ToListAsync();
        return Ok(rows);
    }

    // GET /api/events/5/results - authenticated users can view event results.
    [Authorize(Roles = "Organiser,Participant")]
    [HttpGet("/api/events/{eventId:int}/results")]
    public async Task<IActionResult> ForEvent(int eventId)
    {
        if (!await _db.Events.AnyAsync(e => e.EventID == eventId)) return NotFound();
        var rows = await (from result in _db.Results.AsNoTracking()
                          join entry in _db.Enrolments.AsNoTracking() on result.EnrolmentID equals entry.EnrolmentID
                          join person in _db.Users.AsNoTracking() on entry.ParticipantID equals person.UserID
                          join category in _db.Categories.AsNoTracking() on entry.CategoryID equals category.CategoryID
                          where entry.EventID == eventId
                          orderby result.OverallPosition
                          select new
                          {
                              result.ResultID, result.EnrolmentID,
                              person.FirstName, person.LastName,
                              category.CategoryName, result.FinishTime,
                              result.OverallPosition, result.CategoryPosition,
                              result.RecordedAt
                          }).ToListAsync();
        return Ok(rows);
    }

    // PUT /api/results/1 - owning Organiser updates positions and time.
    [Authorize(Roles = "Organiser")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ResultUpdateDto request)
    {
        var result = await _db.Results.FirstOrDefaultAsync(r => r.ResultID == id);
        if (result == null) return NotFound();
        var entry = await _db.Enrolments.AsNoTracking()
            .FirstOrDefaultAsync(e => e.EnrolmentID == result.EnrolmentID);
        if (entry == null) return NotFound();
        if (!await OwnsEvent(entry.EventID)) return Forbid();
        if (!ValidFinishTime(request.FinishTime)) return BadRequest(new { Message = "Invalid finish time." });

        result.FinishTime = request.FinishTime;
        result.OverallPosition = request.OverallPosition;
        result.CategoryPosition = request.CategoryPosition;
        await _db.SaveChangesAsync();
        return Ok(result);
    }

    // DELETE /api/results/1 - owning Organiser may remove incorrect results.
    [Authorize(Roles = "Organiser")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _db.Results.FirstOrDefaultAsync(r => r.ResultID == id);
        if (result == null) return NotFound();
        var entry = await _db.Enrolments.AsNoTracking()
            .FirstOrDefaultAsync(e => e.EnrolmentID == result.EnrolmentID);
        if (entry == null) return NotFound();
        if (!await OwnsEvent(entry.EventID)) return Forbid();
        _db.Results.Remove(result);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private async Task<bool> OwnsEvent(int eventId)
    {
        int id = User.GetRaceDayUserId();
        return await _db.Events.AnyAsync(e => e.EventID == eventId && e.OrganiserID == id);
    }

    private static bool ValidFinishTime(TimeSpan time) =>
        time > TimeSpan.Zero && time < TimeSpan.FromDays(1);
}
