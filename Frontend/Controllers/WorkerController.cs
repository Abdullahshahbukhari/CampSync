using Frontend.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Frontend.Controllers
{
    public class WorkerController : Controller
    {
        private readonly HttpClient _httpClient;

        public WorkerController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress =new Uri("https://localhost:7232/");
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
        private async Task<List<UserDropdownModel>> GetWorkerUsersAsync()
        {
            var response = await _httpClient.GetAsync("api/Auth/Workers");
            if (!response.IsSuccessStatusCode)
                return new List<UserDropdownModel>();
            return await response.Content.ReadFromJsonAsync<List<UserDropdownModel>>()
                   ?? new List<UserDropdownModel>();
        }

        [HttpGet]
        public async Task<IActionResult> WorkerList(string searchType,int? searchId)
        {
            if (!SetAuthorizationHeader())
            {
                return RedirectToAction("Login", "Auth");
            }

            var role = HttpContext.Session.GetString("role") ?? "";
            if (!role.Equals("Admin", StringComparison.OrdinalIgnoreCase) &&
                !role.Equals("Supervisor", StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            ViewBag.Role = role;
            ViewBag.IsSupervisor =role.Equals("Supervisor", StringComparison.OrdinalIgnoreCase);
            ViewBag.SearchType = searchType;
            ViewBag.SearchId = searchId;
            if (string.IsNullOrEmpty(searchType))
            {
                return View(new List<WorkerResponseModel>());
            }


            if (searchType == "worker")
            {
                if (!searchId.HasValue)
                {
                    ViewBag.Error ="Please enter a Worker ID.";
                    return View(new List<WorkerResponseModel>());
                }
                var response = await _httpClient.GetAsync($"api/Worker/{searchId.Value}");
                if (response.IsSuccessStatusCode)
                {
                    var worker =await response.Content.ReadFromJsonAsync<WorkerResponseModel>();
                    var result =new List<WorkerResponseModel>();
                    if (worker != null)
                    {
                        result.Add(worker);
                    }
                    return View(result);
                }

                ViewBag.Error =response.StatusCode == System.Net.HttpStatusCode.Forbidden
                        ? "You can only view workers from your assigned camp."
                        : "No worker found with this Worker ID.";
                return View(new List<WorkerResponseModel>());
            }

            if (searchType == "camp")
            {
                if (!searchId.HasValue)
                {
                    ViewBag.Error ="Please select a Camp.";
                    return View(new List<WorkerResponseModel>());
                }
                var response = await _httpClient.GetAsync($"api/Worker/by-camp/{searchId.Value}");
                if (response.IsSuccessStatusCode)
                {
                    var workers =await response.Content.ReadFromJsonAsync<List<WorkerResponseModel>>();
                    if (workers == null || !workers.Any())
                    {
                        ViewBag.Error ="No workers found in this Camp.";
                    }
                    return View(workers ??new List<WorkerResponseModel>());
                }

                ViewBag.Error ="No workers found in this Camp.";
                return View(new List<WorkerResponseModel>());
            }

            return View(new List<WorkerResponseModel>());
        }

        [HttpGet]
        public async Task<IActionResult> WorkerById(int id)
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

            var response = await _httpClient.GetAsync(
                $"api/Worker/{id}"
            );

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] ="Worker does not exist.";

                return RedirectToAction(nameof(WorkerList));
            }
            var worker =await response.Content.ReadFromJsonAsync<WorkerResponseModel>();
            if (worker == null)
            {
                TempData["Error"] ="Worker does not exist.";
                return RedirectToAction(nameof(WorkerList));
            }
            return View(worker);
        }

        [HttpGet]
        public async Task<IActionResult> CreateWorker()
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

            ViewBag.WorkerUsers = await GetWorkerUsersAsync();
            return View(new WorkerDTO());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateWorker(WorkerDTO model)
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

            var response =await _httpClient.PostAsJsonAsync("api/Worker/CreateWorker",model);
            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] ="Worker created successfully.";
                return RedirectToAction(nameof(WorkerList));
            }
            var error =await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", error);
            ViewBag.WorkerUsers = await GetWorkerUsersAsync();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditWorker(int id)
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
            var response = await _httpClient.GetAsync($"api/Worker/{id}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] ="Worker does not exist.";
                return RedirectToAction(nameof(WorkerList));
            }
            var worker =await response.Content.ReadFromJsonAsync<WorkerResponseModel>();
            if (worker == null)
            {
                return RedirectToAction(nameof(WorkerList));
            }
            ViewBag.WorkerId = id;
            return View(worker);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateWorker(
            int id,
            WorkerDTO model)
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
            var response =await _httpClient.PutAsJsonAsync($"api/Worker/{id}",model);
            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] ="Worker updated successfully.";
                return RedirectToAction(nameof(WorkerList));
            }
            TempData["Error"] =await response.Content.ReadAsStringAsync();
            return RedirectToAction(nameof(EditWorker),new { id });
        }

        [HttpGet]
        public async Task<IActionResult> GetCamps()
        {
            if (!SetAuthorizationHeader())
            {
                return Unauthorized();
            }

            var role = HttpContext.Session.GetString("role");
            if (role != "Admin" && role != "Supervisor")
            {
                return Unauthorized();
            }
            var response =await _httpClient.GetAsync("api/Camp/CampList");
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode);
            }
            var camps =await response.Content.ReadFromJsonAsync<List<CampResponseModel>>();
            return Json(camps ?? new List<CampResponseModel>());
        }


        [HttpGet]
        public async Task<IActionResult> GetRoomsByCamp(int campId)
        {
            if (!SetAuthorizationHeader())
            {
                return Unauthorized();
            }
            var role = HttpContext.Session.GetString("role");
            if (role != "Admin")
            {
                return Unauthorized();
            }

            var response =await _httpClient.GetAsync($"api/Room/by-camp/{campId}");
            if (!response.IsSuccessStatusCode)
            {
                return BadRequest();
            }
            var rooms =await response.Content.ReadFromJsonAsync<List<RoomResponseModel>>();
            return Json(rooms);
        }

        [HttpGet]
        public async Task<IActionResult> GetBuses()
        {
            if (!SetAuthorizationHeader())
            {
                return Unauthorized();
            }
            var role = HttpContext.Session.GetString("role");
            if (role != "Admin")
            {
                return Unauthorized();
            }
            var response =await _httpClient.GetAsync("api/Bus");
            if (!response.IsSuccessStatusCode)
            {
                return BadRequest();
            }
            var buses =await response.Content.ReadFromJsonAsync<List<BusResponseModel>>();
            return Json(buses);
        }

        [HttpGet]
        public async Task<IActionResult> GetBedsByRoom(int roomId, int? currentBedId = null)
        {
            if (!SetAuthorizationHeader())
            {
                return Unauthorized();
            }

            var role = HttpContext.Session.GetString("role");
            if (role != "Admin")
            {
                return Unauthorized();
            }
            var response = await _httpClient.GetAsync($"api/Bed/by-room/{roomId}");
            if (!response.IsSuccessStatusCode)
            {
                return BadRequest();
            }
            var beds =await response.Content.ReadFromJsonAsync<List<BedResponseModel>>();
            if (beds == null)
            {
                beds = new List<BedResponseModel>();
            }
            var availableBeds = beds.Where(x =>!x.IsOccupied ||(currentBedId.HasValue &&x.Id == currentBedId.Value)).ToList();
            return Json(availableBeds);
        }


        [HttpGet]
        public async Task<IActionResult> DeleteWorker(int id)
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

            var response = await _httpClient.GetAsync(
                $"api/Worker/{id}"
            );

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] ="Worker does not exist.";
                return RedirectToAction(nameof(WorkerList));
            }
            var worker =await response.Content.ReadFromJsonAsync<WorkerResponseModel>();
            return View(worker);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
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
            var response =await _httpClient.DeleteAsync($"api/Worker/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] ="Worker deleted successfully.";
            }
            else
            {
                TempData["Error"] =await response.Content.ReadAsStringAsync();
            }
            return RedirectToAction(
                nameof(WorkerList)
            );
        }
    }
}