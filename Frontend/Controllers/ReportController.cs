using Frontend.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Frontend.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReportController : Controller
    {
        private readonly HttpClient _httpClient;

        public ReportController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7232/");
        }

        private bool SetAuthorizationHeader()
        {
            var token = HttpContext.Session.GetString("token");
            if (string.IsNullOrWhiteSpace(token))
                return false;

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
            return true;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            if (!SetAuthorizationHeader())
                return RedirectToAction("Login", "Auth");

            try
            {
                var summary = await _httpClient.GetFromJsonAsync<DashboardSummaryModel>("api/Report/dashboard-summary");
                var attendance = await _httpClient.GetFromJsonAsync<List<AttendancePercentageModel>>("api/Report/attendance-percentage");
                var beds = await _httpClient.GetFromJsonAsync<List<AttendancePercentageModel>>("api/Report/Bed_Occupancy");
                var buses = await _httpClient.GetFromJsonAsync<List<BusUtilizationModel>>("api/Report/bus-utilization");
                var leaves = await _httpClient.GetFromJsonAsync<List<LeaveTrendModel>>("api/Report/leave-trends");

                return View(new ReportDashboardViewModel
                {
                    Summary = summary ?? new(),
                    AttendanceByCamp = attendance ?? new(),
                    BedOccupancy = beds ?? new(),
                    BusUtilization = buses ?? new(),
                    LeaveTrends = leaves ?? new()
                });
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View(new ReportDashboardViewModel());
            }
        }
    }

    public class ReportDashboardViewModel
    {
        public DashboardSummaryModel Summary { get; set; } = new();
        public List<AttendancePercentageModel> AttendanceByCamp { get; set; } = new();
        public List<AttendancePercentageModel> BedOccupancy { get; set; } = new();
        public List<BusUtilizationModel> BusUtilization { get; set; } = new();
        public List<LeaveTrendModel> LeaveTrends { get; set; } = new();
    }
}
