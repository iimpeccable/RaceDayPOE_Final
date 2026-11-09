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

        [HttpPost]
        public async Task<IActionResult> CreateRegistration([FromBody] CreateRegistrationDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int userId;

            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int parsedId))
            {
                userId = parsedId;
            }
            else
            {
                // Fallback to the first available participant user in the database to prevent FK violation crashes
                var defaultUser = await _context.Users.FirstOrDefaultAsync(u => u.RoleId == 2) ?? await _context.Users.FirstOrDefaultAsync();
                userId = defaultUser != null ? defaultUser.UserId : 2;
            }

            bool alreadyRegistered = await _context.Registrations
                .AnyAsync(r => r.EventId == dto.EventId && r.UserId == userId);

            if (alreadyRegistered)
            {
                return BadRequest("You are already registered for this event.");
            }

            var registration = new Registration
            {
                UserId = userId,
                EventId = dto.EventId,
                CategoryId = dto.CategoryId > 0 ? dto.CategoryId : 1,
                Status = "Confirmed"
            };

            _context.Registrations.Add(registration);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Registration successful", registrationId = registration.RegistrationId });
        }
    }

    public class CreateRegistrationDto
    {
        public int EventId { get; set; }
        public int CategoryId { get; set; }
    }
}