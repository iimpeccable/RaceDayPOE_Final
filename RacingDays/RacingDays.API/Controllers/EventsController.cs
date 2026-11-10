using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RacingDays.API.Data;
using RacingDays.API.Models;
using System.Security.Claims;

namespace RacingDays.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly RacingDaysDbContext _context;

        public EventsController(RacingDaysDbContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value
                          ?? User.FindFirst("email")?.Value;

            if (!string.IsNullOrEmpty(emailClaim))
            {
                var user = _context.Users.FirstOrDefault(u => u.Email == emailClaim);
                if (user != null) return user.UserId;
            }

            var existingOrganiser = _context.Users.FirstOrDefault(u => u.RoleId == 1);
            if (existingOrganiser != null) return existingOrganiser.UserId;

            var devUser = new User
            {
                Name = "Dev Organiser",
                Email = "dummyadmin@racingdays.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                RoleId = 1,
                Age = 30,
                Contact = "0820000000"
            };

            _context.Users.Add(devUser);
            _context.SaveChanges();

            return devUser.UserId;
        }

        [HttpGet]
        public async Task<IActionResult> GetEvents([FromQuery] string? search, [FromQuery] string? eventType)
        {
            var query = _context.Events
                .Include(e => e.Organizer)
                .Include(e => e.Category)
                .Include(e => e.Registrations)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(e => e.Name.Contains(search) || e.Location.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(eventType))
            {
                query = query.Where(e => e.EventType == eventType);
            }

            var events = await query.ToListAsync();

            var eventViewModels = events.Select(e => new
            {
                EventId = e.EventId,
                Name = e.Name,
                Description = e.Description,
                Date = e.Date,
                Location = e.Location,
                Distance = e.Distance,
                EventType = e.EventType,
                CategoryId = e.CategoryId ?? 1,
                BannerImageUrl = e.BannerImageUrl,
                OrganizerName = e.Organizer != null ? e.Organizer.Name : "Dev Organiser",
                TotalEnrolments = e.Registrations.Count
            }).ToList();

            return Ok(eventViewModels);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEvent(int id)
        {
            var ev = await _context.Events
                .Include(e => e.Organizer)
                .Include(e => e.Category)
                .Include(e => e.Registrations)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (ev == null) return NotFound();

            var result = new
            {
                EventId = ev.EventId,
                Name = ev.Name,
                Description = ev.Description,
                Date = ev.Date,
                Location = ev.Location,
                Distance = ev.Distance,
                EventType = ev.EventType,
                CategoryId = ev.CategoryId ?? 1,
                BannerImageUrl = ev.BannerImageUrl,
                OrganizerName = ev.Organizer != null ? ev.Organizer.Name : "Dev Organiser",
                TotalEnrolments = ev.Registrations.Count
            };

            return Ok(result);
        }

        [HttpGet("organiser/dashboard")]
        public async Task<IActionResult> GetOrganiserDashboard()
        {
            int organiserId = GetCurrentUserId();

            var events = await _context.Events
                .Include(e => e.Category)
                .Include(e => e.Registrations)
                .Where(e => e.OrganizerId == organiserId)
                .OrderByDescending(e => e.Date)
                .ToListAsync();

            var eventViewModels = events.Select(e => new
            {
                EventId = e.EventId,
                Name = e.Name,
                Description = e.Description,
                Date = e.Date,
                Location = e.Location,
                Distance = e.Distance,
                EventType = e.EventType,
                CategoryId = e.CategoryId ?? 1,
                BannerImageUrl = e.BannerImageUrl,
                TotalEnrolments = e.Registrations.Count
            }).ToList();

            var model = new
            {
                TotalEventsCreated = eventViewModels.Count,
                TotalEnrolments = eventViewModels.Sum(e => e.TotalEnrolments),
                Events = eventViewModels
            };

            return Ok(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] Event model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            model.OrganizerId = GetCurrentUserId();
            _context.Events.Add(model);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEvent), new { id = model.EventId }, model);
        }

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
            existing.CategoryId = model.CategoryId;

            if (!string.IsNullOrEmpty(model.BannerImageUrl))
            {
                existing.BannerImageUrl = model.BannerImageUrl;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();

            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("{id}/participants")]
        public async Task<IActionResult> GetEventParticipants(int id)
        {
            var roster = await _context.Registrations
                .Include(r => r.User)
                .Include(r => r.Category)
                .Include(r => r.Results)
                .Where(r => r.EventId == id)
                .Select(r => new
                {
                    RegistrationId = r.RegistrationId,
                    ParticipantName = r.User != null ? r.User.Name : "Participant",
                    ParticipantEmail = r.User != null ? r.User.Email : "",
                    Age = r.User != null ? r.User.Age : 0,
                    Contact = r.User != null ? r.User.Contact : "",
                    CategoryName = r.Category != null ? r.Category.Name : "General",
                    RegistrationStatus = r.Status,
                    RegistrationDate = r.Date,
                    HasResultRecorded = r.Results.Any(),
                    RecordedFinishTime = r.Results.FirstOrDefault() != null ? r.Results.FirstOrDefault()!.FinishTime.ToString(@"hh\:mm\:ss") : null,
                    RecordedPosition = r.Results.FirstOrDefault() != null ? (int?)r.Results.FirstOrDefault()!.Position : null
                })
                .ToListAsync();

            return Ok(roster);
        }
    }
}