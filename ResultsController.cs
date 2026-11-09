using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RacingDays.API.Data;
using RacingDays.API.Models;

namespace RacingDays.API.Controllers
{
    // API Controller managing race completion results and standings
    [ApiController]
    [Route("api/[controller]")]
    public class ResultsController : ControllerBase
    {
        private readonly RacingDaysDbContext _context;

        public ResultsController(RacingDaysDbContext context)
        {
            _context = context;
        }

        // GET: api/results/event/5
        // Retrieves published finish times and standings for a specific event
        [HttpGet("event/{eventId}")]
        public async Task<IActionResult> GetResultsByEvent(int eventId)
        {
            var results = await _context.Results
                .Include(r => r.Registration)
                    .ThenInclude(reg => reg.User)
                .Include(r => r.Registration)
                    .ThenInclude(reg => reg.Category)
                .Where(r => r.Registration != null && r.Registration.EventId == eventId)
                .OrderBy(r => r.Position)
                .Select(r => new
                {
                    r.ResultId,
                    r.RegistrationId,
                    ParticipantName = r.Registration != null && r.Registration.User != null ? r.Registration.User.Name : "Athlete",
                    CategoryName = r.Registration != null && r.Registration.Category != null ? r.Registration.Category.Name : "Open Division",
                    r.Position,
                    r.FinishTime
                })
                .ToListAsync();

            return Ok(results);
        }

        // POST: api/results
        // Publishes or updates an athlete's finishing position and time
        [HttpPost]
        public async Task<IActionResult> SubmitResult([FromBody] SubmitResultDto dto)
        {
            var registration = await _context.Registrations.FindAsync(dto.RegistrationId);
            if (registration == null)
                return NotFound("Participant registration record not found.");

            var existingResult = await _context.Results
                .FirstOrDefaultAsync(r => r.RegistrationId == dto.RegistrationId);

            if (existingResult != null)
            {
                existingResult.Position = dto.Position;
                existingResult.FinishTime = dto.FinishTime;
            }
            else
            {
                var newResult = new Result
                {
                    RegistrationId = dto.RegistrationId,
                    Position = dto.Position,
                    FinishTime = dto.FinishTime
                };
                _context.Results.Add(newResult);
            }

            registration.Status = "Completed";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Race result published successfully!" });
        }
    }

    // DTO payload for submitting results
    public class SubmitResultDto
    {
        public int RegistrationId { get; set; }
        public int Position { get; set; }
        public TimeSpan FinishTime { get; set; }
    }
}