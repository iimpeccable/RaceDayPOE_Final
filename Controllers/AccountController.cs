using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RacingDays.Web.Models;
using System.Text;

namespace RacingDays.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly HttpClient _apiClient;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _apiClient = httpClientFactory.CreateClient("RacingDaysApi");
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _apiClient.PostAsync("api/auth/login", content);
            if (response.IsSuccessStatusCode)
            {
                var resString = await response.Content.ReadAsStringAsync();
                // Store auth token / user session info
                HttpContext.Session.SetString("AuthToken", resString);
                HttpContext.Session.SetString("UserEmail", model.Email);
                
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Invalid login attempt.");
            return View(model);
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _apiClient.PostAsync("api/auth/register", content);
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Registration successful! Please sign in.";
                return RedirectToAction("Login");
            }

            var err = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", $"Registration failed: {err}");
            return View(model);
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}