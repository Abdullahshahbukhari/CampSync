using Frontend.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Frontend.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public UserController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private HttpClient GetClient()
        {
            var client = _httpClientFactory.CreateClient();

            client.BaseAddress = new Uri("https://localhost:7232/");

            var token = HttpContext.Session.GetString("token");

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =new AuthenticationHeaderValue("Bearer", token);
            }
            return client;
        }

        [HttpGet]
        public IActionResult CreateUser()
        {
            return View(new CreateUserModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(CreateUserModel model)
        {
            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword","Passwords do not match.");
            }
            var allowedRoles = new[]
            {
                "Worker",
                "Supervisor",
                "Driver"
            };
            if (!allowedRoles.Contains(model.Role,StringComparer.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("Role","Invalid role.");
            }
            if (!ModelState.IsValid)
                return View(model);
            var client = GetClient();
            var response = await client.PostAsJsonAsync("api/Auth/register-by-admin",model);
            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error =await response.Content.ReadAsStringAsync();
                return View(model);
            }
            TempData["Success"] =$"{model.Role} account created successfully.";
            return RedirectToAction(nameof(CreateUser));
        }
    }
}