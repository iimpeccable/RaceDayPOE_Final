using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RacingDays.API.Data;
using RacingDays.API.Models;
using System.Security.Claims;

namespace RacingDays.API.Controllers
{
    // API Controller managing event lifecycle and organiser participant rosters
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly RacingDaysDbContext _context;

        public EventsController(RacingDaysDbContext context)
        {
            _context = context;
        }

        // Resolves user ID from JWT token claim or defaults to dummyadmin (ID 1)
        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(claim) && int.TryParse(claim, out int id))
            {
                // Verify user actually exists in DB, otherwise fallback to 1
                var exists = _context.Users.Any(u => u.UserId == id);
                if (exists) return id;
            }

            // Default safely to first Organiser user in database (e.g. ID 1)
            var defaultOrganiser = _context.Users.FirstOrDefault(u => u.RoleId == 1) ?? _context.Users.FirstOrDefault();
            return defaultOrganiser != null ? defaultOrganiser.UserId : 1;
        }

        // GET: api/events
        // Retrieves all public race events with enrollment counts
        [HttpGet]
        public async Task<IActionResult> GetAllEvents()
        {
            var events = await _context.Events
                .Include(e => e.Organizer)
                .Select(e => new
                {
                    e.EventId,
                    e.Name,
                    e.Description,
                    e.Date,
                    e.Location,
                    e.Distance,
                    e.EventType,
                    e.BannerImageUrl,
                    e.OrganizerId,
                    OrganizerName = e.Organizer != null ? e.Organizer.Name : "Organiser",
                    TotalEnrolments = _context.Registrations.Count(r => r.EventId == e.EventId)
                })
                .ToListAsync();

            return Ok(events);
        }

        // GET: api/events/5
        // Retrieves single event details with computed enrollment count
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEventById(int id)
        {
            var ev = await _context.Events
                .Include(e => e.Organizer)
                .Where(e => e.EventId == id)
                .Select(e => new
                {
                    e.EventId,
                    e.Name,
                    e.Description,
                    e.Date,
                    e.Location,
                    e.Distance,
                    e.EventType,
                    e.BannerImageUrl,
                    e.OrganizerId,
                    OrganizerName = e.Organizer != null ? e.Organizer.Name : "Organiser",
                    TotalEnrolments = _context.Registrations.Count(r => r.EventId == e.EventId)
                })
                .FirstOrDefaultAsync();

            if (ev == null) return NotFound();
            return Ok(ev);
        }

        // POST: api/events
        // Creates a new race event assigned to current organiser
        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] Event model)
        {
            model.OrganizerId = GetCurrentUserId();
            model.Organizer = null; // Clear navigation property to avoid EF tracking errors

            _context.Events.Add(model);
            await _context.SaveChangesAsync();
            return Ok(model);
        }

        // PUT: api/events/5
        // Updates existing event specifications
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] Event model)
        {
            var existing = await _context.Events.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Name = model.Name;
            existing.Description = model.Description;
            existing.Date = model.Date;
            existing.Location = model.Location;
            existing.Distance = model.Distance;
            existing.EventType = model.EventType;
            if (!string.IsNullOrEmpty(model.BannerImageUrl))
            {
                existing.BannerImageUrl = model.BannerImageUrl;
            }

            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        // DELETE: api/events/5
        // Removes an event record from database
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();

            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Event deleted" });
        }

        // GET: api/events/organiser/dashboard
        // Summarizes organiser host metrics and list of managed events
        [HttpGet("organiser/dashboard")]
        public async Task<IActionResult> GetOrganiserDashboard()
        {
            int organizerId = GetCurrentUserId();

            var events = await _context.Events
                .Where(e => e.OrganizerId == organizerId)
                .Select(e => new
                {
                    e.EventId,
                    e.Name,
                    e.Date,
                    e.Location,
                    e.Distance,
                    e.EventType,
                    TotalEnrolments = _context.Registrations.Count(r => r.EventId == e.EventId)
                })
                .ToListAsync();

            return Ok(new
            {
                TotalEventsCreated = events.Count,
                TotalEnrolments = events.Sum(e => e.TotalEnrolments),
                Events = events
            });
        }

        // GET: api/events/5/participants
        // Fetches registered athletes, including their ages, contact info, and chosen category
        [HttpGet("{id}/participants")]
        public async Task<IActionResult> GetEventParticipants(int id)
        {
            var participants = await _context.Registrations
                .Include(r => r.User)
                .Include(r => r.Category)
                .Where(r => r.EventId == id)
                .Select(r => new
                {
                    r.RegistrationId,
                    ParticipantName = r.User != null ? r.User.Name : "N/A",
                    ParticipantEmail = r.User != null ? r.User.Email : "N/A",
                    ParticipantAge = r.User != null ? r.User.Age : 0,
                    ParticipantContact = r.User != null ? r.User.Contact : "N/A",
                    CategoryName = r.Category != null ? r.Category.Name : "Open Division",
                    r.Status
                })
                .ToListAsync();

            return Ok(participants);
        }
    }
}