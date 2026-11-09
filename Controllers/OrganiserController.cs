using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RacingDays.Web.Models;
using RacingDays.Web.Services;

namespace RacingDays.Web.Controllers
{
    // Controller handling organiser race creation, editing, category dropdowns, and roster inspection
    public class OrganiserController : Controller
    {
        private readonly RacingDaysApiClient _apiClient;

        public OrganiserController(RacingDaysApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        // Helper to load race categories from API into ViewBag
        private async Task LoadCategoriesAsync()
        {
            var response = await _apiClient.GetAsync("api/categories");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                ViewBag.Categories = JsonConvert.DeserializeObject<List<CategoryViewModel>>(json);
            }
            else
            {
                ViewBag.Categories = new List<CategoryViewModel>();
            }
        }

        // GET: /Organiser/Dashboard
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var response = await _apiClient.GetAsync("api/events/organiser/dashboard");
            if (!response.IsSuccessStatusCode)
                return View(new OrganiserDashboardViewModel());

            var json = await response.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<OrganiserDashboardViewModel>(json) ?? new OrganiserDashboardViewModel();
            return View(model);
        }

        // GET: /Organiser/CreateEvent
        [HttpGet]
        public async Task<IActionResult> CreateEvent()
        {
            await LoadCategoriesAsync();
            return View(new CreateEventViewModel());
        }

        // POST: /Organiser/CreateEvent
        [HttpPost]
        public async Task<IActionResult> CreateEvent(CreateEventViewModel model, IFormFile? bannerFile)
        {
            if (!ModelState.IsValid)
            {
                await LoadCategoriesAsync();
                return View(model);
            }

            // Save uploaded banner image file locally to wwwroot/uploads
            if (bannerFile != null && bannerFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(bannerFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await bannerFile.CopyToAsync(stream);
                }

                model.BannerImageUrl = $"/uploads/{fileName}";
            }

            var response = await _apiClient.PostAsync("api/events", model);
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Event created successfully!";
                return RedirectToAction(nameof(Dashboard));
            }

            var errorMsg = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", $"Failed to create event: {errorMsg}");
            await LoadCategoriesAsync();
            return View(model);
        }

        // GET: /Organiser/EditEvent/5
        [HttpGet]
        public async Task<IActionResult> EditEvent(int id)
        {
            var response = await _apiClient.GetAsync($"api/events/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var json = await response.Content.ReadAsStringAsync();
            var ev = JsonConvert.DeserializeObject<CreateEventViewModel>(json);

            await LoadCategoriesAsync();
            ViewBag.EventId = id;
            return View(ev);
        }

        // POST: /Organiser/EditEvent/5
        [HttpPost]
        public async Task<IActionResult> EditEvent(int id, CreateEventViewModel model, IFormFile? bannerFile)
        {
            if (!ModelState.IsValid)
            {
                await LoadCategoriesAsync();
                ViewBag.EventId = id;
                return View(model);
            }

            if (bannerFile != null && bannerFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(bannerFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await bannerFile.CopyToAsync(stream);
                }

                model.BannerImageUrl = $"/uploads/{fileName}";
            }

            var response = await _apiClient.PutAsync($"api/events/{id}", model);
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Event updated successfully!";
                return RedirectToAction(nameof(Dashboard));
            }

            await LoadCategoriesAsync();
            ViewBag.EventId = id;
            return View(model);
        }

        // POST: /Organiser/DeleteEvent/5
        [HttpPost]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            await _apiClient.DeleteAsync($"api/events/{id}");
            TempData["SuccessMessage"] = "Event deleted successfully.";
            return RedirectToAction(nameof(Dashboard));
        }

        // GET: /Organiser/Participants/5
        // Displays registered participants with names, ages, contacts, and categories
        [HttpGet]
        public async Task<IActionResult> Participants(int id)
        {
            var eventResponse = await _apiClient.GetAsync($"api/events/{id}");
            if (!eventResponse.IsSuccessStatusCode) return NotFound();

            var eventJson = await eventResponse.Content.ReadAsStringAsync();
            var eventDetails = JsonConvert.DeserializeObject<EventViewModel>(eventJson);
            ViewBag.EventName = eventDetails?.Name ?? "Event";

            var rosterResponse = await _apiClient.GetAsync($"api/events/{id}/participants");
            var roster = new List<ParticipantRosterViewModel>();

            if (rosterResponse.IsSuccessStatusCode)
            {
                var rosterJson = await rosterResponse.Content.ReadAsStringAsync();
                roster = JsonConvert.DeserializeObject<List<ParticipantRosterViewModel>>(rosterJson) ?? new List<ParticipantRosterViewModel>();
            }

            return View(roster);
        }
        // GET: /Organiser/CaptureResult?eventId=5&registrationId=10&participantName=John
        [HttpGet]
        public async Task<IActionResult> CaptureResult(int eventId, int registrationId, string participantName)
        {
            var eventResponse = await _apiClient.GetAsync($"api/events/{eventId}");
            var eventName = "Race Event";

            if (eventResponse.IsSuccessStatusCode)
            {
                var json = await eventResponse.Content.ReadAsStringAsync();
                var ev = JsonConvert.DeserializeObject<EventViewModel>(json);
                eventName = ev?.Name ?? eventName;
            }

            var model = new SubmitResultViewModel
            {
                EventId = eventId,
                RegistrationId = registrationId,
                ParticipantName = participantName,
                EventName = eventName
            };

            return View(model);
        }

        // POST: /Organiser/CaptureResult
        [HttpPost]
        public async Task<IActionResult> CaptureResult(SubmitResultViewModel model)
        {
            if (!TimeSpan.TryParse(model.FinishTimeInput, out TimeSpan parsedTime))
            {
                ModelState.AddModelError("FinishTimeInput", "Invalid time format. Please use HH:mm:ss (e.g., 01:15:30)");
                return View(model);
            }

            var payload = new
            {
                RegistrationId = model.RegistrationId,
                Position = model.Position,
                FinishTime = parsedTime
            };

            var response = await _apiClient.PostAsync("api/results", payload);
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = $"Result captured for {model.ParticipantName}!";
                return RedirectToAction("Participants", new { id = model.EventId });
            }

            var err = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", $"Failed to submit result: {err}");
            return View(model);
        }
    }
}