using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs.Enrolments;
using RaceDayAPI.Models;
using RaceDayAPI.Security;

namespace RaceDayAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/enrolments")]
public class EnrolmentsController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    public EnrolmentsController(RaceDayDbContext db) => _db = db;

    // POST /api/enrolments - Participant signs up for a category.
    [Authorize(Roles = "Participant")]
    [HttpPost]
    public async Task<IActionResult> Create(EnrolmentCreateDto request)
    {
        int participantId = User.GetRaceDayUserId();
        var ev = await _db.Events.FirstOrDefaultAsync(e => e.EventID == request.EventID);
        if (ev == null) return NotFound(new { Message = "Event not found." });
        if (DateTime.Now > ev.RegistrationDeadline || DateTime.Now >= ev.EventDate)
            return BadRequest(new { Message = "Registration has closed." });

        var category = await _db.Categories.FirstOrDefaultAsync(c => c.CategoryID == request.CategoryID);
        if (category == null || category.EventID != ev.EventID)
            return BadRequest(new { Message = "Category does not belong to the selected event." });

        // A serializable transaction reduces races when checking capacity.
        await using var tx = _db.Database.IsRelational()
            ? await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable)
            : null; // EF InMemory is used by CI tests and has no transactions.
        if (await _db.Enrolments.AnyAsync(e => e.ParticipantID == participantId &&
            e.EventID == ev.EventID && e.Status != "Cancelled"))
            return Conflict(new { Message = "You are already enrolled in this event." });

        int count = await _db.Enrolments.CountAsync(e => e.CategoryID == category.CategoryID &&
            e.Status != "Cancelled");
        if (count >= category.MaxParticipants)
            return Conflict(new { Message = "Category is full." });

        var enrolment = new Enrolment
        {
            ParticipantID = participantId,
            EventID = ev.EventID,
            CategoryID = category.CategoryID,
            Status = "Confirmed",
            EnrolmentDate = DateTime.Now,
            RaceNumber = "RD" + Guid.NewGuid().ToString("N")[..16]
        };
        _db.Enrolments.Add(enrolment);
        await _db.SaveChangesAsync();
        if (tx != null) await tx.CommitAsync();
        return StatusCode(201, enrolment);
    }

    // GET /api/enrolments/my - Participant's own entries only.
    [Authorize(Roles = "Participant")]
    [HttpGet("my")]
    public async Task<IActionResult> Mine()
    {
        int participantId = User.GetRaceDayUserId();
        var rows = await (from enrolment in _db.Enrolments.AsNoTracking()
                          join ev in _db.Events.AsNoTracking() on enrolment.EventID equals ev.EventID
                          join category in _db.Categories.AsNoTracking() on enrolment.CategoryID equals category.CategoryID
                          where enrolment.ParticipantID == participantId
                          orderby ev.EventDate descending
                          select new
                          {
                              enrolment.EnrolmentID, enrolment.EventID, ev.EventName,
                              ev.EventDate, enrolment.CategoryID, category.CategoryName,
                              enrolment.RaceNumber, enrolment.Status, enrolment.EnrolmentDate
                          }).ToListAsync();
        return Ok(rows);
    }

    // GET /api/events/5/enrolments - ONLY the event's Organiser.
    [Authorize(Roles = "Organiser")]
    [HttpGet("/api/events/{eventId:int}/enrolments")]
    public async Task<IActionResult> ForEvent(int eventId)
    {
        var ev = await _db.Events.AsNoTracking().FirstOrDefaultAsync(e => e.EventID == eventId);
        if (ev == null) return NotFound();
        if (ev.OrganiserID != User.GetRaceDayUserId()) return Forbid();

        var rows = await (from entry in _db.Enrolments.AsNoTracking()
                          join person in _db.Users.AsNoTracking() on entry.ParticipantID equals person.UserID
                          join category in _db.Categories.AsNoTracking() on entry.CategoryID equals category.CategoryID
                          where entry.EventID == eventId
                          orderby entry.EnrolmentDate
                          select new
                          {
                              entry.EnrolmentID, entry.ParticipantID,
                              person.FirstName, person.LastName, person.Email,
                              entry.CategoryID, category.CategoryName,
                              entry.Status, entry.RaceNumber, entry.EnrolmentDate
                          }).ToListAsync();
        return Ok(rows);
    }

    // PUT /api/enrolments/3 - owner Organiser updates status.
    [Authorize(Roles = "Organiser")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, EnrolmentUpdateDto request)
    {
        var entry = await _db.Enrolments.FirstOrDefaultAsync(e => e.EnrolmentID == id);
        if (entry == null) return NotFound();
        if (!await OwnsEvent(entry.EventID)) return Forbid();
        if (await _db.Results.AnyAsync(r => r.EnrolmentID == id) && request.Status == "Cancelled")
            return Conflict(new { Message = "Cannot cancel an enrolment with a recorded result." });

        if (entry.Status == "Cancelled" && request.Status != "Cancelled")
        {
            var category = await _db.Categories.FindAsync(entry.CategoryID);
            int active = await _db.Enrolments.CountAsync(e => e.CategoryID == entry.CategoryID && e.Status != "Cancelled");
            if (category != null && active >= category.MaxParticipants)
                return Conflict(new { Message = "Category is full." });
        }
        entry.Status = request.Status;
        await _db.SaveChangesAsync();
        return Ok(entry);
    }

    // DELETE /api/enrolments/3 - own Participant entry or organiser of that event.
    [Authorize(Roles = "Participant,Organiser")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entry = await _db.Enrolments.FirstOrDefaultAsync(e => e.EnrolmentID == id);
        if (entry == null) return NotFound();
        int userId = User.GetRaceDayUserId();
        bool organiserOwns = User.IsInRole("Organiser") && await OwnsEvent(entry.EventID);
        bool participantOwns = User.IsInRole("Participant") && entry.ParticipantID == userId;
        if (!organiserOwns && !participantOwns) return Forbid();

        if (await _db.Results.AnyAsync(r => r.EnrolmentID == id))
            return Conflict(new { Message = "Cannot delete an enrolment with a recorded result." });
        if (participantOwns && await _db.Events.AnyAsync(e => e.EventID == entry.EventID &&
            e.RegistrationDeadline < DateTime.Now))
            return Conflict(new { Message = "The registration deadline has passed." });

        _db.Enrolments.Remove(entry);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private async Task<bool> OwnsEvent(int eventId)
    {
        int userId = User.GetRaceDayUserId();
        return await _db.Events.AnyAsync(e => e.EventID == eventId && e.OrganiserID == userId);
    }
}
