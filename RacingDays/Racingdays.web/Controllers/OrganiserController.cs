using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RacingDays.Web.Models;
using RacingDays.Web.Services;

namespace RacingDays.Web.Controllers
{
    // Controller handling race creation, editing, category population, roster inspection, and result recording
    public class OrganiserController : Controller
    {
        private readonly RacingDaysApiClient _apiClient;

        public OrganiserController(RacingDaysApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        // Helper method to stream uploaded image through API to Azure Blob Storage
        private async Task<string?> UploadBannerToApiAsync(IFormFile bannerFile)
        {
            if (bannerFile == null || bannerFile.Length == 0) return null;

            using var content = new MultipartFormDataContent();
            using var stream = bannerFile.OpenReadStream();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(bannerFile.ContentType);

            content.Add(fileContent, "file", bannerFile.FileName);

            var response = await _apiClient.PostAsync("api/uploads/banner", content);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<dynamic>(json);
                return result?.url?.ToString();
            }

            return null;
        }

        // Helper method to populate race categories into ViewBag for form dropdowns
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

            // Upload Banner Image to Azure Blob Storage via API if present
            if (bannerFile != null && bannerFile.Length > 0)
            {
                var uploadedUrl = await UploadBannerToApiAsync(bannerFile);
                if (!string.IsNullOrEmpty(uploadedUrl))
                {
                    model.BannerImageUrl = uploadedUrl;
                }
            }

            // Build direct API payload mapping to ensure OrganizerId & CategoryId bind cleanly
            var apiPayload = new
            {
                Name = model.Name,
                Description = model.Description,
                Date = model.Date,
                Location = model.Location,
                Distance = model.Distance,
                EventType = model.EventType,
                CategoryId = model.CategoryId > 0 ? model.CategoryId : (int?)null,
                BannerImageUrl = model.BannerImageUrl
            };

            var response = await _apiClient.PostAsync("api/events", apiPayload);
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
                var uploadedUrl = await UploadBannerToApiAsync(bannerFile);
                if (!string.IsNullOrEmpty(uploadedUrl))
                {
                    model.BannerImageUrl = uploadedUrl;
                }
            }

            var apiPayload = new
            {
                Name = model.Name,
                Description = model.Description,
                Date = model.Date,
                Location = model.Location,
                Distance = model.Distance,
                EventType = model.EventType,
                CategoryId = model.CategoryId > 0 ? model.CategoryId : (int?)null,
                BannerImageUrl = model.BannerImageUrl
            };

            var response = await _apiClient.PutAsync($"api/events/{id}", apiPayload);
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
        [HttpGet]
        public async Task<IActionResult> Participants(int id)
        {
            var eventResponse = await _apiClient.GetAsync($"api/events/{id}");
            if (!eventResponse.IsSuccessStatusCode) return NotFound();

            var eventJson = await eventResponse.Content.ReadAsStringAsync();
            var eventDetails = JsonConvert.DeserializeObject<EventViewModel>(eventJson);

            ViewBag.EventName = eventDetails?.Name ?? "Event";
            ViewBag.EventId = id;

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