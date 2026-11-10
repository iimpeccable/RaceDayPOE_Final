using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace RacingDays.Web.Services
{
    // Centralized API Client Service responsible for HTTP communication between Web MVC and Web API
    public class RacingDaysApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RacingDaysApiClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _httpClient.BaseAddress = new Uri("https://localhost:7001/"); // Base Web API Endpoint URL
        }

        // Attaches JWT bearer authentication header from active Session state
        private void AddAuthHeader()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWTToken");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        // Asynchronous HTTP GET helper
        public async Task<HttpResponseMessage> GetAsync(string endpoint)
        {
            AddAuthHeader();
            return await _httpClient.GetAsync(endpoint);
        }

        // Asynchronous HTTP POST helper for JSON payloads
        public async Task<HttpResponseMessage> PostAsync(string endpoint, object data)
        {
            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            AddAuthHeader();
            return await _httpClient.PostAsync(endpoint, content);
        }

        // Asynchronous HTTP PUT helper for JSON updates
        public async Task<HttpResponseMessage> PutAsync(string endpoint, object data)
        {
            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            AddAuthHeader();
            return await _httpClient.PutAsync(endpoint, content);
        }

        // Asynchronous HTTP DELETE helper
        public async Task<HttpResponseMessage> DeleteAsync(string endpoint)
        {
            AddAuthHeader();
            return await _httpClient.DeleteAsync(endpoint);
        }
    }
}