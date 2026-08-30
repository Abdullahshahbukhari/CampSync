using Frontend.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Frontend.Controllers
{
    public class BedController : Controller
    {
        private readonly HttpClient _httpClient;

        public BedController(IHttpClientFactory httpClientFactory)
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

            var camps =  await response.Content.ReadFromJsonAsync<List<CampResponseModel>>();
            return camps ?? new List<CampResponseModel>();
        }


        private async Task<List<RoomResponseModel>> GetRoomsByCampAsync(int campId)
        {
            var response =await _httpClient.GetAsync($"api/Room/by-camp/{campId}");
            if (!response.IsSuccessStatusCode)
            {
                return new List<RoomResponseModel>();
            }
            var rooms =await response.Content.ReadFromJsonAsync<List<RoomResponseModel>>();
            return rooms ?? new List<RoomResponseModel>();
        }

        [HttpGet]
        public async Task<IActionResult> BedList(int? campId,int? roomId)
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
            ViewBag.SelectedRoomId = roomId;

            if (campId.HasValue)
            {
                ViewBag.Rooms =await GetRoomsByCampAsync(campId.Value);
            }
            else
            {
                ViewBag.Rooms =new List<RoomResponseModel>();
            }
            if (!roomId.HasValue)
            {
                return View(new List<BedResponseModel>());
            }

            var response =await _httpClient.GetAsync($"api/Bed/by-room/{roomId.Value}");
            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error =await response.Content.ReadAsStringAsync();
                return View(new List<BedResponseModel>());
            }

            var beds =await response.Content.ReadFromJsonAsync<List<BedResponseModel>>();
            return View(beds ?? new List<BedResponseModel>());
        }

        [HttpGet]
        public async Task<IActionResult> AvailableBeds(int? campId)
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

            ViewBag.Camps =await GetCampsForDropdownAsync();

            ViewBag.SelectedCampId = campId;

            if (!campId.HasValue)
            {
                return View(new List<BedResponseModel>());
            }
            var response =await _httpClient.GetAsync($"api/Bed/by-camp/{campId.Value}");
            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error =await response.Content.ReadAsStringAsync();
                return View(new List<BedResponseModel>());
            }
            var beds =await response.Content.ReadFromJsonAsync<List<BedResponseModel>>();
            return View(beds ?? new List<BedResponseModel>());
        }

        [HttpGet]
        public async Task<IActionResult> CreateBed(int? campId,int? roomId)
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
            ViewBag.Camps =await GetCampsForDropdownAsync();
            ViewBag.SelectedCampId = campId;
            if (campId.HasValue)
            {
                ViewBag.Rooms =await GetRoomsByCampAsync(campId.Value);
            }
            else
            {
                ViewBag.Rooms =new List<RoomResponseModel>();
            }
            var model = new BedCreateModel();
            if (roomId.HasValue)
            {
                model.RoomId = roomId.Value;
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBed(BedCreateModel model,int? campId)
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
                ViewBag.Camps =await GetCampsForDropdownAsync();
                ViewBag.SelectedCampId = campId;
                ViewBag.Rooms = campId.HasValue? await GetRoomsByCampAsync(campId.Value): new List<RoomResponseModel>();
                return View(model);
            }
            var response =await _httpClient.PostAsJsonAsync("api/Bed",model);
            var message =await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = message;
                ViewBag.Camps =await GetCampsForDropdownAsync();
                ViewBag.SelectedCampId = campId;
                ViewBag.Rooms = campId.HasValue? await GetRoomsByCampAsync(campId.Value): new List<RoomResponseModel>();
                return View(model);
            }

            TempData["Success"] = message;
            return RedirectToAction(nameof(BedList),new
                {
                    campId,roomId = model.RoomId
                });
        }

        [HttpGet]
        public async Task<IActionResult> EditBed(
            int id,
            int bedNo,
            int roomId,
            int? campId)
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

            ViewBag.BedId = id;
            ViewBag.Camps =await GetCampsForDropdownAsync();
            ViewBag.SelectedCampId = campId;
            ViewBag.Rooms = campId.HasValue? await GetRoomsByCampAsync(campId.Value): new List<RoomResponseModel>();
            var model = new BedUpdateModel
            {
                BedNo = bedNo,
                RoomId = roomId
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBed(
            int id,
            BedUpdateModel model,
            int? campId)
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
                ViewBag.BedId = id;
                ViewBag.Camps =await GetCampsForDropdownAsync();
                ViewBag.SelectedCampId = campId;
                ViewBag.Rooms = campId.HasValue? await GetRoomsByCampAsync(campId.Value): new List<RoomResponseModel>();
                return View(model);
            }

            var response =await _httpClient.PutAsJsonAsync($"api/Bed/{id}",model);
            var message = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                ViewBag.BedId = id;
                ViewBag.Error = message;
                ViewBag.Camps =await GetCampsForDropdownAsync();
                ViewBag.SelectedCampId = campId;
                ViewBag.Rooms = campId.HasValue? await GetRoomsByCampAsync(campId.Value): new List<RoomResponseModel>();
                return View(model);
            }

            TempData["Success"] = message;

            return RedirectToAction(
                nameof(BedList),
                new
                {
                    campId,
                    roomId = model.RoomId
                });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBed(
            int id,
            int roomId,
            int? campId)
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

            var response =await _httpClient.DeleteAsync($"api/Bed/{id}");
            var message =await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = message;
            }
            else
            {
                TempData["Success"] = message;
            }

            return RedirectToAction(nameof(BedList),new
                {
                    campId,
                    roomId
                });
        }
    }
}