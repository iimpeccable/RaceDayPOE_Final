using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RacingDays.Web.Models;
using RacingDays.Web.Services;

namespace RacingDays.Web.Controllers
{
    // Web Participant Controller for public event directory & registration
    public class ParticipantController : Controller
    {
        private readonly RacingDaysApiClient _apiClient;

        public ParticipantController(RacingDaysApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        // 1. Directory of all upcoming race events
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var response = await _apiClient.GetAsync("api/events");
            var events = new List<EventViewModel>();

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                events = JsonConvert.DeserializeObject<List<EventViewModel>>(json) ?? new List<EventViewModel>();
            }

            return View(events);
        }

        // 2. View specific event details
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var response = await _apiClient.GetAsync($"api/events/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var json = await response.Content.ReadAsStringAsync();
            var eventDetails = JsonConvert.DeserializeObject<EventViewModel>(json);

            return View(eventDetails);
        }

        // 3. Process race enrolment for logged-in participant
        [HttpPost]
        public async Task<IActionResult> RegisterForEvent(int eventId)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(userRole))
            {
                TempData["ErrorMessage"] = "Please log in to register for race events.";
                return RedirectToAction("Login", "Account");
            }

            // Post registration mapping payload to backend API
            var registrationPayload = new
            {
                EventId = eventId,
                CategoryId = 1 // Default Open Distance Category
            };

            var response = await _apiClient.PostAsync("api/registrations", registrationPayload);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Successfully registered for the event! See you on race day!";
            }
            else
            {
                var err = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Registration failed: {err}";
            }

            return RedirectToAction(nameof(Details), new { id = eventId });
        }


        // GET: /Participant/MyEnrolments
        [HttpGet]
        public async Task<IActionResult> MyEnrolments()
        {
            var response = await _apiClient.GetAsync("api/registrations/my");
            List<RegistrationViewModel> enrolments = new();

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                enrolments = JsonConvert.DeserializeObject<List<RegistrationViewModel>>(json) ?? new();
            }

            return View(enrolments);
        }

        // GET: /Participant/MyResults
        [HttpGet]
        public async Task<IActionResult> MyResults()
        {
            var response = await _apiClient.GetAsync("api/results/my");
            List<ParticipantResultViewModel> results = new();

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                results = JsonConvert.DeserializeObject<List<ParticipantResultViewModel>>(json) ?? new();
            }

            return View(results);
        }

        // GET: /Participant/Profile
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var response = await _apiClient.GetAsync("api/users/profile");
            UserProfileViewModel profile = new();

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                profile = JsonConvert.DeserializeObject<UserProfileViewModel>(json) ?? new();
            }

            return View(profile);
        }

        // POST: /Participant/Profile
        [HttpPost]
        public async Task<IActionResult> Profile(UserProfileViewModel model, IFormFile? profilePictureFile)
        {
            // Handle optional profile picture upload via API
            var response = await _apiClient.PutAsync("api/users/profile", model);
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction("Profile");
            }

            ModelState.AddModelError("", "Failed to update profile.");
            return View(model);
        }
    }
}