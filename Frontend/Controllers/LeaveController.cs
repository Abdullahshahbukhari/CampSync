using Frontend.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Frontend.Controllers
{
    public class LeaveController : Controller
    {
        private readonly HttpClient _httpClient;

        public LeaveController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7232/");
        }

        private bool SetAuthorizationHeader()
        {
            var token = HttpContext.Session.GetString("token");
            if (string.IsNullOrWhiteSpace(token)) return false;

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
            return true;
        }

        private string Role => HttpContext.Session.GetString("role") ?? "";

        private bool IsLoggedIn() => SetAuthorizationHeader();
        private bool IsManagement() => Role == "Admin" || Role == "Supervisor";

        [HttpGet]
        public IActionResult LeaveHub()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            return View();
        }

        [HttpGet]
        public IActionResult ApplyLeave()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            if (!new[] { "Admin", "Supervisor", "Worker", "Driver" }.Contains(Role)) return Forbid();

            var today = DateOnly.FromDateTime(DateTime.Today);
            return View(new LeaveCreateModel { FromDate = today, ToDate = today });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyLeave(LeaveCreateModel model)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            if (!new[] { "Admin", "Supervisor", "Worker", "Driver" }.Contains(Role)) return Forbid();

            if (!ModelState.IsValid) return View(model);

            var response = await _httpClient.PostAsJsonAsync("api/Leave", model);
            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = await response.Content.ReadAsStringAsync();
                return RedirectToAction(nameof(MyLeaves));
            }

            ModelState.AddModelError("", await response.Content.ReadAsStringAsync());
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> MyLeaves()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            if (!new[] { "Admin", "Supervisor", "Worker", "Driver" }.Contains(Role)) return Forbid();

            var response = await _httpClient.GetAsync("api/Leave/my-leaves");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = await response.Content.ReadAsStringAsync();
                return View(new List<LeaveResponseModel>());
            }

            return View(await response.Content.ReadFromJsonAsync<List<LeaveResponseModel>>() ?? new());
        }

        [HttpGet]
        public async Task<IActionResult> PendingLeaves()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            if (!IsManagement()) return Forbid();

            var response = await _httpClient.GetAsync("api/Leave/Pending");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = await response.Content.ReadAsStringAsync();
                return View(new List<LeaveResponseModel>());
            }

            return View(await response.Content.ReadFromJsonAsync<List<LeaveResponseModel>>() ?? new());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveLeave(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            if (!IsManagement()) return Forbid();

            var response = await _httpClient.PutAsync($"api/Leave/approve/{id}", null);
            if (response.IsSuccessStatusCode)
                TempData["Success"] = await response.Content.ReadAsStringAsync();
            else
                TempData["Error"] = await response.Content.ReadAsStringAsync();

            return RedirectToAction(nameof(PendingLeaves));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectLeave(int id, LeaveDecisionModel model)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            if (!IsManagement()) return Forbid();

            var response = await _httpClient.PutAsJsonAsync(
                $"api/Leave/{id}/reject",
                model ?? new LeaveDecisionModel());

            if (response.IsSuccessStatusCode)
                TempData["Success"] = await response.Content.ReadAsStringAsync();
            else
                TempData["Error"] = await response.Content.ReadAsStringAsync();

            return RedirectToAction(nameof(PendingLeaves));
        }

        [HttpGet]
        public async Task<IActionResult> LeavesByWorker(int workerId)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            if (!IsManagement()) return Forbid();

            var response = await _httpClient.GetAsync($"api/Leave/by-worker/{workerId}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = await response.Content.ReadAsStringAsync();
                return RedirectToAction(nameof(LeaveHub));
            }

            ViewBag.WorkerId = workerId;
            return View(await response.Content.ReadFromJsonAsync<List<LeaveResponseModel>>() ?? new());
        }

        [HttpGet]
        public async Task<IActionResult> Summary(int? workerId)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            if (!new[] { "Admin", "Supervisor", "Worker", "Driver" }.Contains(Role)) return Forbid();
            if (workerId.HasValue && !IsManagement()) return Forbid();

            var url = workerId.HasValue
                ? $"api/Leave/summary/{workerId.Value}"
                : "api/Leave/summary";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = await response.Content.ReadAsStringAsync();
                return View(new LeaveSummaryModel());
            }

            return View(await response.Content.ReadFromJsonAsync<LeaveSummaryModel>() ?? new LeaveSummaryModel());
        }
    }
}
