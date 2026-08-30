using Frontend.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Frontend.Controllers
{
    public class RoomController : Controller
    {
        private readonly HttpClient _httpClient;

        public RoomController(IHttpClientFactory httpClientFactory)
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

        private async Task<List<CampResponseModel>> GetCampsForDropdownAsync()
        {
            var response = await _httpClient.GetAsync("api/Camp/CampList");

            if (!response.IsSuccessStatusCode)
            {
                return new List<CampResponseModel>();
            }

            var camps = await response.Content.ReadFromJsonAsync<List<CampResponseModel>>();
            return camps ?? new List<CampResponseModel>();
        }

        [HttpGet]
        public async Task<IActionResult> RoomList(int? campId)
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

            ViewBag.Camps = await GetCampsForDropdownAsync();
            ViewBag.SelectedCampId = campId;

            if (campId == null)
            {
                return View(new List<RoomResponseModel>());
            }

            var response = await _httpClient.GetAsync($"api/Room/by-camp/{campId}");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = await response.Content.ReadAsStringAsync();
                return View(new List<RoomResponseModel>());
            }

            var rooms = await response.Content.ReadFromJsonAsync<List<RoomResponseModel>>();
            return View(rooms ?? new List<RoomResponseModel>());
        }


        [HttpGet]
        public async Task<IActionResult> CreateRoom(int? campId)
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

            ViewBag.Camps = await GetCampsForDropdownAsync();

            var model = new RoomCreateModel();
            if (campId.HasValue)
            {
                model.CampId = campId.Value;
            }

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRoom(RoomCreateModel model)
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
                ViewBag.Camps = await GetCampsForDropdownAsync();
                return View(model);
            }

            var response = await _httpClient.PostAsJsonAsync("api/Room", model);

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = await response.Content.ReadAsStringAsync();
                ViewBag.Camps = await GetCampsForDropdownAsync();
                return View(model);
            }

            TempData["Success"] = await response.Content.ReadAsStringAsync();
            return RedirectToAction(nameof(RoomList), new { campId = model.CampId });
        }

        [HttpGet]
        public async Task<IActionResult> Roombyid(int id)
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

            var response = await _httpClient.GetAsync($"api/Room/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = await response.Content.ReadAsStringAsync();
                return RedirectToAction(nameof(RoomList));
            }

            var room = await response.Content.ReadFromJsonAsync<RoomResponseModel>();

            if (room == null)
            {
                TempData["Error"] = "Room not found.";
                return RedirectToAction(nameof(RoomList));
            }

            return View(room);
        }

        [HttpGet]
        public async Task<IActionResult> EditRoom(int id)
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

            var response = await _httpClient.GetAsync($"api/Room/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = await response.Content.ReadAsStringAsync();
                return RedirectToAction(nameof(RoomList));
            }

            var room = await response.Content.ReadFromJsonAsync<RoomResponseModel>();

            if (room == null)
            {
                TempData["Error"] = "Room not found.";
                return RedirectToAction(nameof(RoomList));
            }

            var model = new RoomUpdateModel
            {
                RoomNo = room.RoomNo,
                CampId = room.CampId
            };

            ViewBag.RoomId = room.Id;
            // ViewBag.CurrentCampName = room.CampName;
            ViewBag.Camps = await GetCampsForDropdownAsync();

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRoom(int id, RoomUpdateModel model)
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
                ViewBag.RoomId = id;
                ViewBag.Camps = await GetCampsForDropdownAsync();
                return View(model);
            }

            var response = await _httpClient.PutAsJsonAsync($"api/Room/{id}", model);

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.RoomId = id;
                ViewBag.Error = await response.Content.ReadAsStringAsync();
                ViewBag.Camps = await GetCampsForDropdownAsync();
                return View(model);
            }

            TempData["Success"] = await response.Content.ReadAsStringAsync();
            return RedirectToAction(nameof(Roombyid), new { id });
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

            var response = await _httpClient.GetAsync($"api/Room/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = await response.Content.ReadAsStringAsync();
                return RedirectToAction(nameof(RoomList));
            }

            var room = await response.Content.ReadFromJsonAsync<RoomResponseModel>();

            if (room == null)
            {
                TempData["Error"] = "Room not found.";
                return RedirectToAction(nameof(RoomList));
            }

            return View(room);
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

            var response = await _httpClient.DeleteAsync($"api/Room/{id}");
            var message = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = message;
                return RedirectToAction(nameof(Roombyid), new { id });
            }

            TempData["Success"] = message;
            return RedirectToAction(nameof(RoomList));
        }
    }
}
