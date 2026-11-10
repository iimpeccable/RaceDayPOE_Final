using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RacingDays.API.Data;
using RacingDays.API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RacingDays.API.Controllers
{
    // API Controller handling BCrypt user registration, login verification, and JWT generation
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly RacingDaysDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(RacingDaysDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterApiDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest("Email address is already registered.");

            var newUser = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Age = dto.Age,
                Contact = dto.Contact,
                RoleId = dto.RoleId > 0 ? dto.RoleId : 2 // Default to 2 (Participant)
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Registration successful" });
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            // Eagerly load Role entity to ensure role name claims are resolved
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Invalid email address or password.");

            // Resolve role name cleanly
            string roleName = user.Role?.Name ?? (user.RoleId == 1 ? "Organiser" : "Participant");

            var token = GenerateJwtToken(user, roleName);

            return Ok(new
            {
                token = token,
                user = new
                {
                    userId = user.UserId,
                    name = user.Name,
                    email = user.Email,
                    role = roleName
                }
            });
        }

        // Generates JWT token with security claims
        private string GenerateJwtToken(User user, string roleName)
        {
            var jwtKey = _config["Jwt:Key"] ?? "RacingDaysSuperSecretKeyForJWTAuth2026!";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, roleName)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"] ?? "RacingDaysAPI",
                audience: _config["Jwt:Audience"] ?? "RacingDaysUsers",
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class RegisterApiDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Contact { get; set; } = string.Empty;
        public int RoleId { get; set; }
    }

    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}