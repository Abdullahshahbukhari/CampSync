using Frontend.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Frontend.Controllers
{
    public class BusController : Controller
    {
        private readonly HttpClient _httpClient;

        public BusController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7232/");
        }

        private bool SetAuthorizationHeader()
        {
            var token = HttpContext.Session.GetString("token");

            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            return true;
        }

        private async Task<List<DriverResponseModel>> GetDriversAsync()
        {
            var response = await _httpClient.GetAsync("api/Auth/Drivers");

            if (!response.IsSuccessStatusCode)
            {
                return new List<DriverResponseModel>();
            }

            var drivers = await response.Content
                .ReadFromJsonAsync<List<DriverResponseModel>>();

            return drivers ?? new List<DriverResponseModel>();
        }

        [HttpGet]
        public async Task<IActionResult> MyBus()
        {
            if (!SetAuthorizationHeader())
                return RedirectToAction("Login", "Auth");

            var role = HttpContext.Session.GetString("role");
            if (role != "Driver" && role != "Worker")
                return Forbid();

            var response = await _httpClient.GetAsync("api/Bus/my-bus");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = await response.Content.ReadAsStringAsync();
                return View(new BusResponseModel());
            }

            var bus = await response.Content.ReadFromJsonAsync<BusResponseModel>();
            return View(bus ?? new BusResponseModel());
        }

        [HttpGet]
        public async Task<IActionResult> BusList(int? searchId)
        {
            if (!SetAuthorizationHeader())
            {
                return RedirectToAction("Login", "Auth");
            }

            var role = HttpContext.Session.GetString("role");
            if (role != "Admin" && role != "Supervisor")
            {
                return Forbid();
            }

            if (searchId.HasValue)
            {
                var response = await _httpClient.GetAsync($"api/Bus/{searchId.Value}");

                if (response.IsSuccessStatusCode)
                {
                    var bus = await response.Content.ReadFromJsonAsync<BusResponseModel>();

                    var result = new List<BusResponseModel>();

                    if (bus != null)
                    {
                        result.Add(bus);
                    }

                    return View(result);
                }

                ViewBag.Error = "No bus found with this Bus ID.";

                return View(new List<BusResponseModel>());
            }

            var busesResponse = await _httpClient.GetAsync("api/Bus");

            if (!busesResponse.IsSuccessStatusCode)
            {
                ViewBag.Error =
                    await busesResponse.Content.ReadAsStringAsync();

                return View(new List<BusResponseModel>());
            }

            var buses = await busesResponse.Content
                .ReadFromJsonAsync<List<BusResponseModel>>();

            return View(buses ?? new List<BusResponseModel>());
        }

        [HttpGet]
        public async Task<IActionResult> CreateBus()
        {
            if (!SetAuthorizationHeader())
            {
                return RedirectToAction("Login", "Auth");
            }

            var role = HttpContext.Session.GetString("role");
            if (role != "Admin")
            {
                return Forbid();
            }

            ViewBag.Drivers = await GetDriversAsync();

            return View(new BusCreateModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBus(BusCreateModel model)
        {
            if (!SetAuthorizationHeader())
            {
                return RedirectToAction("Login", "Auth");
            }

            var role = HttpContext.Session.GetString("role");
            if (role != "Admin")
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Drivers = await GetDriversAsync();
                return View(model);
            }

            var response = await _httpClient
                .PostAsJsonAsync("api/Bus", model);

            var message = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = message;
                ViewBag.Drivers = await GetDriversAsync();

                return View(model);
            }

            TempData["Success"] = message;

            return RedirectToAction(nameof(BusList));
        }

        [HttpGet]
        public async Task<IActionResult> Busbyid(int id)
        {
            if (!SetAuthorizationHeader())
            {
                return RedirectToAction("Login", "Auth");
            }

            var role = HttpContext.Session.GetString("role");
            if (role != "Admin" && role != "Supervisor")
            {
                return Forbid();
            }

            var response = await _httpClient.GetAsync($"api/Bus/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = await response.Content.ReadAsStringAsync();

                return RedirectToAction(nameof(BusList));
            }

            var bus = await response.Content
                .ReadFromJsonAsync<BusResponseModel>();

            if (bus == null)
            {
                TempData["Error"] = "Bus not found.";

                return RedirectToAction(nameof(BusList));
            }

            return View(bus);
        }

        [HttpGet]
        public async Task<IActionResult> EditBus(int id)
        {
            if (!SetAuthorizationHeader())
            {
                return RedirectToAction("Login", "Auth");
            }

            var role = HttpContext.Session.GetString("role");


            if (role != "Admin" && role != "Supervisor")
            {
                return Forbid();
            }

            var response = await _httpClient.GetAsync($"api/Bus/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = await response.Content.ReadAsStringAsync();

                return RedirectToAction(nameof(BusList));
            }

            var bus = await response.Content
                .ReadFromJsonAsync<BusResponseModel>();

            if (bus == null)
            {
                TempData["Error"] = "Bus not found.";

                return RedirectToAction(nameof(BusList));
            }

            ViewBag.BusId = bus.Id;
            ViewBag.Drivers = await GetDriversAsync();
            ViewBag.CurrentDriverName = bus.DriverName;

            var model = new BusUpdateModel
            {
                BusNo = bus.BusNo,
                DriverId = bus.DriverId,
                Capacity = bus.Capacity
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBus(int id, BusUpdateModel model)
        {
            if (!SetAuthorizationHeader())
            {
                return RedirectToAction("Login", "Auth");
            }

            var role = HttpContext.Session.GetString("role");

            if (role != "Admin" && role != "Supervisor")
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.BusId = id;
                ViewBag.Drivers = await GetDriversAsync();

                return View(model);
            }

            var response = await _httpClient
                .PutAsJsonAsync($"api/Bus/{id}", model);

            var message = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = message;
                ViewBag.BusId = id;
                ViewBag.Drivers = await GetDriversAsync();

                return View(model);
            }

            TempData["Success"] = message;

            return RedirectToAction(
                nameof(Busbyid),
                new { id }
            );
        }


        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            if (!SetAuthorizationHeader())
            {
                return RedirectToAction("Login", "Auth");
            }

            var role = HttpContext.Session.GetString("role");
            if (role != "Admin")
            {
                return Forbid();
            }

            var response = await _httpClient.GetAsync($"api/Bus/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = await response.Content.ReadAsStringAsync();

                return RedirectToAction(nameof(BusList));
            }

            var bus = await response.Content
                .ReadFromJsonAsync<BusResponseModel>();

            if (bus == null)
            {
                TempData["Error"] = "Bus not found.";

                return RedirectToAction(nameof(BusList));
            }

            return View(bus);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!SetAuthorizationHeader())
            {
                return RedirectToAction("Login", "Auth");
            }

            var role = HttpContext.Session.GetString("role");

            if (role != "Admin")
            {
                return Forbid();
            }

            var response = await _httpClient.DeleteAsync($"api/Bus/{id}");

            var message = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = message;

                var busResponse = await _httpClient.GetAsync($"api/Bus/{id}");

                if (!busResponse.IsSuccessStatusCode)
                {
                    TempData["Error"] = message;
                    return RedirectToAction(nameof(BusList));
                }

                var bus = await busResponse.Content
                    .ReadFromJsonAsync<BusResponseModel>();

                return View("Delete", bus);
            }

            TempData["Success"] = message;

            return RedirectToAction(nameof(BusList));
        }
    }
}