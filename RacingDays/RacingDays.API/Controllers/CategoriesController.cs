using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RacingDays.API.Data;
using RacingDays.API.Models;

namespace RacingDays.API.Controllers
{
    // API Controller serving race categories (e.g., Distance vs. Age divisions)
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly RacingDaysDbContext _context;

        public CategoriesController(RacingDaysDbContext context)
        {
            _context = context;
        }

        // GET: api/categories
        // Returns the list of all seeded categories for dropdown pickers
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Categories.ToListAsync();
            return Ok(categories);
        }
    }
}