using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RacingDays.Web.Models;
using System.Text;

namespace RacingDays.Web.Controllers
{
    // Controller managing user authentication, registration, and session token storage
    public class AccountController : Controller
    {
        private readonly HttpClient _apiClient;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _apiClient = httpClientFactory.CreateClient("RacingDaysApi");

            // Safety fallback if BaseAddress is not populated
            if (_apiClient.BaseAddress == null)
            {
                _apiClient.BaseAddress = new Uri("https://localhost:7001/");
            }
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login() => View();

        // POST: /Account/Login
        // Handles standard database user authentication from API response payload
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var jsonPayload = JsonConvert.SerializeObject(model);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await _apiClient.PostAsync("api/auth/login", content);
            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();

                // Deserialize structured JSON response from AuthController
                var authResult = JsonConvert.DeserializeObject<AuthApiResponse>(responseString);

                if (authResult != null && !string.IsNullOrEmpty(authResult.Token))
                {
                    // 1. Store clean JWT bearer token for API Client HTTP headers
                    HttpContext.Session.SetString("JWTToken", authResult.Token);

                    // 2. Store User Email and Role in session for _Layout navbar & view rendering
                    var userEmail = authResult.User?.Email ?? model.Email;
                    var userRole = authResult.User?.Role ?? "Participant";

                    HttpContext.Session.SetString("UserEmail", userEmail);
                    HttpContext.Session.SetString("UserRole", userRole);

                    // 3. Route user to appropriate dashboard based on returned role
                    if (userRole.Equals("Organiser", StringComparison.OrdinalIgnoreCase))
                    {
                        return RedirectToAction("Dashboard", "Organiser");
                    }
                    return RedirectToAction("Index", "Participant");
                }
            }

            ModelState.AddModelError("", "Invalid email address or password.");
            return View(model);
        }

        // GET: /Account/DevLoginOrganiser
        // Fast-track bypass login action for testing Organiser views
        [HttpGet]
        public IActionResult DevLoginOrganiser()
        {
            HttpContext.Session.SetString("JWTToken", "DEV_BYPASS_TOKEN_ORGANISER");
            HttpContext.Session.SetString("UserEmail", "dummyadmin@racingdays.com");
            HttpContext.Session.SetString("UserRole", "Organiser");
            return RedirectToAction("Dashboard", "Organiser");
        }

        // GET: /Account/DevLoginParticipant
        // Fast-track bypass login action for testing Participant views
        [HttpGet]
        public IActionResult DevLoginParticipant()
        {
            HttpContext.Session.SetString("JWTToken", "DEV_BYPASS_TOKEN_PARTICIPANT");
            HttpContext.Session.SetString("UserEmail", "dummyparticipant@racingdays.com");
            HttpContext.Session.SetString("UserRole", "Participant");
            return RedirectToAction("Index", "Participant");
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register() => View();

        // POST: /Account/Register
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var jsonPayload = JsonConvert.SerializeObject(model);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await _apiClient.PostAsync("api/auth/register", content);
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Registration successful! Please sign in with your credentials.";
                return RedirectToAction("Login");
            }

            var err = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", $"Registration failed: {err}");
            return View(model);
        }

        // GET: /Account/Logout
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }

    // Response models matching API JSON structure
    public class AuthApiResponse
    {
        public string Token { get; set; } = string.Empty;
        public AuthUserResponse? User { get; set; }
    }

    public class AuthUserResponse
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}