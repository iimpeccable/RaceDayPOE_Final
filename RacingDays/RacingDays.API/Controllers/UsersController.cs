using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RacingDays.API.Data;
using RacingDays.API.Models;
using System.Security.Claims;

namespace RacingDays.API.Controllers
{
    // API Controller managing user athlete profiles
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly RacingDaysDbContext _context;

        public UsersController(RacingDaysDbContext context)
        {
            _context = context;
        }

        // Resolves active User ID from JWT token claim or defaults to participant 2
        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(claim) && int.TryParse(claim, out int id))
            {
                if (_context.Users.Any(u => u.UserId == id)) return id;
            }

            var defaultParticipant = _context.Users.FirstOrDefault(u => u.RoleId == 2) ?? _context.Users.FirstOrDefault();
            return defaultParticipant != null ? defaultParticipant.UserId : 2;
        }

        // GET: api/users/profile
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            int userId = GetCurrentUserId();
            var user = await _context.Users.FindAsync(userId);

            if (user == null) return NotFound("User profile not found.");

            return Ok(new
            {
                user.UserId,
                user.Name,
                user.Email,
                user.Age,
                user.Contact
            });
        }

        // PUT: api/users/profile
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UserProfileUpdateDto dto)
        {
            int userId = GetCurrentUserId();
            var user = await _context.Users.FindAsync(userId);

            if (user == null) return NotFound("User profile not found.");

            user.Name = dto.Name;
            user.Age = dto.Age;
            user.Contact = dto.Contact;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Profile updated successfully!" });
        }
    }

    public class UserProfileUpdateDto
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Contact { get; set; } = string.Empty;
    }
}