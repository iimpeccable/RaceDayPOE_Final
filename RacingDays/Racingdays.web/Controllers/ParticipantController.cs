using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RacingDays.Web.Models;
using RacingDays.Web.Services;

namespace RacingDays.Web.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly RacingDaysApiClient _apiClient;

        public ParticipantController(RacingDaysApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? search, string? eventType)
        {
            var queryString = $"api/events?search={search}&eventType={eventType}";
            var response = await _apiClient.GetAsync(queryString);

            var events = new List<EventViewModel>();
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                events = JsonConvert.DeserializeObject<List<EventViewModel>>(json) ?? new List<EventViewModel>();
            }

            ViewBag.CurrentSearch = search;
            ViewBag.CurrentType = eventType;
            return View(events);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var response = await _apiClient.GetAsync($"api/events/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var json = await response.Content.ReadAsStringAsync();
            var eventDetails = JsonConvert.DeserializeObject<EventViewModel>(json);
            if (eventDetails == null) return NotFound();

            return View(eventDetails);
        }

        [HttpPost]
        public async Task<IActionResult> RegisterForEvent(int eventId, int categoryId)
        {
            var payload = new { EventId = eventId, CategoryId = categoryId };
            var response = await _apiClient.PostAsync("api/registrations", payload);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Successfully registered for event!";
                return RedirectToAction(nameof(MyEnrolments));
            }

            var err = await response.Content.ReadAsStringAsync();
            TempData["ErrorMessage"] = $"Registration failed: {err}";
            return RedirectToAction(nameof(Details), new { id = eventId });
        }

        [HttpGet]
        public async Task<IActionResult> MyEnrolments()
        {
            var response = await _apiClient.GetAsync("api/registrations/my-enrolments");
            var enrolments = new List<EnrolmentViewModel>();

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                enrolments = JsonConvert.DeserializeObject<List<EnrolmentViewModel>>(json) ?? new List<EnrolmentViewModel>();
            }

            return View(enrolments);
        }

        [HttpGet]
        public async Task<IActionResult> MyResults()
        {
            var response = await _apiClient.GetAsync("api/results/my-results");
            var results = new List<ParticipantResultViewModel>();

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                results = JsonConvert.DeserializeObject<List<ParticipantResultViewModel>>(json) ?? new List<ParticipantResultViewModel>();
            }

            return View(results);
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var response = await _apiClient.GetAsync("api/users/profile");
            if (!response.IsSuccessStatusCode) return RedirectToAction("Login", "Account");

            var json = await response.Content.ReadAsStringAsync();
            var profile = JsonConvert.DeserializeObject<UserProfileViewModel>(json);
            if (profile == null) return RedirectToAction("Login", "Account");

            return View(profile);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(UserProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var response = await _apiClient.PutAsync("api/users/profile", model);
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction(nameof(Profile));
            }

            var err = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", $"Failed to update profile: {err}");
            return View(model);
        }
    }
}