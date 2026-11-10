using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RacingDays.API.Data;
using RacingDays.API.Models;
using System.Security.Claims;

namespace RacingDays.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResultsController : ControllerBase
    {
        private readonly RacingDaysDbContext _context;

        public ResultsController(RacingDaysDbContext context)
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
            return participant?.UserId ?? 1;
        }

        [HttpPost]
        public async Task<IActionResult> RecordResult([FromBody] Result model)
        {
            var reg = await _context.Registrations.FindAsync(model.RegistrationId);
            if (reg == null) return NotFound("Registration not found.");

            var existing = await _context.Results.FirstOrDefaultAsync(r => r.RegistrationId == model.RegistrationId);
            if (existing != null)
            {
                existing.Position = model.Position;
                existing.FinishTime = model.FinishTime;
            }
            else
            {
                _context.Results.Add(model);
            }

            await _context.SaveChangesAsync();
            return Ok(model);
        }

        [HttpGet("my-results")]
        public async Task<IActionResult> GetMyResults()
        {
            int userId = GetCurrentUserId();

            var results = await _context.Results
                .Include(r => r.Registration)
                    .ThenInclude(reg => reg.Event)
                .Include(r => r.Registration)
                    .ThenInclude(reg => reg.Category)
                .Where(r => r.Registration.UserId == userId)
                .Select(r => new
                {
                    ResultId = r.ResultId,
                    EventName = r.Registration.Event.Name,
                    EventDate = r.Registration.Event.Date,
                    CategoryName = r.Registration.Category != null ? r.Registration.Category.Name : "General",
                    Position = r.Position,
                    FinishTime = r.FinishTime.ToString(@"hh\:mm\:ss")
                })
                .ToListAsync();

            return Ok(results);
        }
    }
}