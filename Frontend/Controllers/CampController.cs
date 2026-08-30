using Frontend.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Frontend.Controllers
{
    public class CampController : Controller
    {
        private readonly HttpClient _httpClient;

        public CampController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();

            _httpClient.BaseAddress =
                new Uri("https://localhost:7232/");
        }

        private bool SetAuthorizationHeader()
        {
            var token =
                HttpContext.Session.GetString("token");

            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            _httpClient.DefaultRequestHeaders.Authorization =new AuthenticationHeaderValue(  "Bearer",token);
            return true;
        }


        private IActionResult LoginRedirect()
        {
            return RedirectToAction("Login","Auth");
        }


        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("role")== "Admin";
        }


        private bool IsAdminOrSupervisor()
        {
            var role =HttpContext.Session.GetString("role");
            return role == "Admin"|| role == "Supervisor";
        }

        [HttpGet]
        public async Task<IActionResult> CampList()
        {
            if (!SetAuthorizationHeader())
            {
                return LoginRedirect();
            }

            if (!IsAdminOrSupervisor())
            {
                return RedirectToAction("AccessDenied","Auth"
                );
            }

            ViewBag.IsSupervisor =
                HttpContext.Session.GetString("role")
                    ?.Equals("Supervisor", StringComparison.OrdinalIgnoreCase) == true;

            var response =await _httpClient.GetAsync("api/Camp/CampList");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error =await response.Content.ReadAsStringAsync();
                return View(new List<CampResponseModel>()
                );
            }

            var camps =await response.Content.ReadFromJsonAsync<List<CampResponseModel>>();
            return View(camps ?? new List<CampResponseModel>());
        }

        [HttpGet]
        public async Task<IActionResult> CreateCamp()
        {
            if (!SetAuthorizationHeader())
            {
                return LoginRedirect();
            }

            if (!IsAdmin())
            {
                return RedirectToAction(
                    "AccessDenied",
                    "Auth"
                );
            }

            await LoadAvailableSupervisors();

            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCamp(
            CampCreateModel model
        )
        {
            if (!SetAuthorizationHeader())
            {
                return LoginRedirect();
            }

            if (!IsAdmin())
            {
                return RedirectToAction(
                    "AccessDenied",
                    "Auth"
                );
            }

            if (!ModelState.IsValid)
            {
                await LoadAvailableSupervisors();
                return View(model);
            }

            var response =
                await _httpClient.PostAsJsonAsync(
                    "api/Camp",
                    model
                );

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error =
                    await response.Content
                        .ReadAsStringAsync();

                await LoadAvailableSupervisors();
                return View(model);
            }

            var success = await response.Content
                .ReadFromJsonAsync<CampCreateSuccessModel>();

            TempData["Success"] = success?.Message
                ?? "Camp created and Supervisor assigned successfully.";

            return RedirectToAction(
                nameof(CampList)
            );
        }

        [HttpGet]
        public async Task<IActionResult> Campbyid(
            int id
        )
        {
            if (!SetAuthorizationHeader())
            {
                return LoginRedirect();
            }

            if (!IsAdminOrSupervisor())
            {
                return RedirectToAction( "AccessDenied","Auth");
            }

            var response =
                await _httpClient.GetAsync($"api/Camp/Campbyid/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] =await response.Content.ReadAsStringAsync();
                return RedirectToAction(nameof(CampList));
            }
            var camp =await response.Content.ReadFromJsonAsync<CampResponseModel>();
            if (camp == null)
            {
                TempData["Error"] ="Camp not found.";
                return RedirectToAction(nameof(CampList));
            }

            return View(camp);
        }

        [HttpGet]
        public async Task<IActionResult> EditCamp(int id)
        {
            if (!SetAuthorizationHeader())
            {
                return LoginRedirect();
            }

            if (!IsAdmin())
            {
                return RedirectToAction(
                    "AccessDenied",
                    "Auth"
                );
            }

            var response =await _httpClient.GetAsync($"api/Camp/Campbyid/{id}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] =await response.Content.ReadAsStringAsync();
                return RedirectToAction(nameof(CampList));
            }

            var camp =await response.Content.ReadFromJsonAsync<CampResponseModel>();
            if (camp == null)
            {
                TempData["Error"] ="Camp not found.";
                return RedirectToAction(nameof(CampList));
            }
            var model =new CampUpdateModel
                {
                    Name = camp.Name,
                    Location = camp.Location
                };
            ViewBag.CampId = camp.Id;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCamp(int id,CampUpdateModel model
        )
        {
            if (!SetAuthorizationHeader())
            {
                return LoginRedirect();
            }

            if (!IsAdmin())
            {
                return RedirectToAction(
                    "AccessDenied",
                    "Auth"
                );
            }

            if (!ModelState.IsValid)
            {
                ViewBag.CampId = id;

                return View(model);
            }

            var response =await _httpClient.PutAsJsonAsync($"api/Camp/Campbyid/{id}",model);

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.CampId = id;
                ViewBag.Error =await response.Content.ReadAsStringAsync();
                return View(model);
            }

            TempData["Success"] =await response.Content.ReadAsStringAsync();
            return RedirectToAction(nameof(Campbyid),new { id });
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            if (!SetAuthorizationHeader())
            {
                return LoginRedirect();
            }

            if (!IsAdmin())
            {
                return RedirectToAction(
                    "AccessDenied",
                    "Auth"
                );
            }

            var response =await _httpClient.GetAsync($"api/Camp/Campbyid/{id}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] =await response.Content.ReadAsStringAsync();
                return RedirectToAction(nameof(CampList));
            }
            var camp =await response.Content.ReadFromJsonAsync<CampResponseModel>();
            if (camp == null)
            {
                TempData["Error"] ="Camp not found.";
                return RedirectToAction(nameof(CampList));
            }

            return View(camp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int id
        )
        {
            if (!SetAuthorizationHeader())
            {
                return LoginRedirect();
            }

            if (!IsAdmin())
            {
                return RedirectToAction(
                    "AccessDenied",
                    "Auth"
                );
            }

            var response =
                await _httpClient.DeleteAsync(
                    $"api/Camp/Dlete/{id}"
                );

            var message =
                await response.Content
                    .ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = message;

                return RedirectToAction(
                    nameof(Campbyid),
                    new { id }
                );
            }

            TempData["Success"] = message;

            return RedirectToAction(
                nameof(CampList)
            );
        }

        private async Task LoadAvailableSupervisors()
        {
            var response = await _httpClient.GetAsync("api/Supervisor/Available");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Supervisors = new List<SupervisorAvailableModel>();
                return;
            }

            var supervisors = await response.Content
                .ReadFromJsonAsync<List<SupervisorAvailableModel>>();

            ViewBag.Supervisors = supervisors ?? new List<SupervisorAvailableModel>();
        }

    }
}