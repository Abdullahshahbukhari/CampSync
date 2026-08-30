using Frontend.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Frontend.Controllers
{
    public class ProfileController : Controller
    {
        private readonly HttpClient _httpClient;
        public ProfileController(IHttpClientFactory factory)
        {
            _httpClient = factory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7232/");
        }

        [HttpGet]
        public async Task<IActionResult> MyProfile()
        {
            var token = HttpContext.Session.GetString("token");
            if (string.IsNullOrWhiteSpace(token)) return RedirectToAction("Login", "Auth");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync("api/User/my-profile");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = await response.Content.ReadAsStringAsync();

                return View(
                    new MyProfileModel
                    {
                        Name = User.Identity?.Name ?? "",
                        Email = "",
                        Role = HttpContext.Session.GetString("role") ?? ""
                    });
            }
            return View(await response.Content.ReadFromJsonAsync<MyProfileModel>() ?? new MyProfileModel());
        }
    }
}
