using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RacingDays.API.Data;
using RacingDays.API.Models;
using System.Security.Claims;

namespace RacingDays.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegistrationsController : ControllerBase
    {
        private readonly RacingDaysDbContext _context;

        public RegistrationsController(RacingDaysDbContext context)
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

            var participant = _context.Users.FirstOrDefault(u => u.RoleId == 2);
            if (participant != null) return participant.UserId;

            var devParticipant = new User
            {
                Name = "Dev Participant",
                Email = "participant@racingdays.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("User123!"),
                RoleId = 2,
                Age = 25,
                Contact = "0830000000"
            };

            _context.Users.Add(devParticipant);
            _context.SaveChanges();

            return devParticipant.UserId;
        }

        [HttpPost]
        public async Task<IActionResult> CreateRegistration([FromBody] RegistrationRequest request)
        {
            int userId = GetCurrentUserId();

            var existing = await _context.Registrations
                .FirstOrDefaultAsync(r => r.EventId == request.EventId && r.UserId == userId);

            if (existing != null)
                return BadRequest("Participant is already registered for this event.");

            int categoryIdToUse = request.CategoryId;
            if (categoryIdToUse <= 0)
            {
                var ev = await _context.Events.FindAsync(request.EventId);
                categoryIdToUse = ev?.CategoryId ?? 1;
            }

            var registration = new Registration
            {
                EventId = request.EventId,
                UserId = userId,
                CategoryId = categoryIdToUse,
                Date = DateTime.Now,
                Status = "Confirmed"
            };

            _context.Registrations.Add(registration);
            await _context.SaveChangesAsync();

            return Ok(registration);
        }

        [HttpGet("my-enrolments")]
        public async Task<IActionResult> GetMyEnrolments()
        {
            int userId = GetCurrentUserId();

            var enrolments = await _context.Registrations
                .Include(r => r.Event)
                .Include(r => r.Category)
                .Where(r => r.UserId == userId)
                .Select(r => new
                {
                    RegistrationId = r.RegistrationId,
                    EventId = r.EventId,
                    EventName = r.Event.Name,
                    EventDate = r.Event.Date,
                    Location = r.Event.Location,
                    CategoryName = r.Category != null ? r.Category.Name : "General",
                    Status = r.Status,
                    RegistrationDate = r.Date,
                    BannerImageUrl = r.Event.BannerImageUrl
                })
                .ToListAsync();

            return Ok(enrolments);
        }
    }

    public class RegistrationRequest
    {
        public int EventId { get; set; }
        public int CategoryId { get; set; }
    }
}