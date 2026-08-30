using Frontend.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Frontend.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly HttpClient _httpClient;

        public AttendanceController(IHttpClientFactory httpClientFactory)
        {
            _httpClient =httpClientFactory.CreateClient();

            _httpClient.BaseAddress =new Uri("https://localhost:7232/");
        }


        private bool SetAuthorizationHeader()
        {
            var token =HttpContext.Session.GetString("token");

            if (string.IsNullOrEmpty(token))
            {
                return false;
            }
            _httpClient.DefaultRequestHeaders.Authorization =new AuthenticationHeaderValue("Bearer",token);
            return true;
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
                return Json(new List<CampResponseModel>());
            }

            var camps =await response.Content.ReadFromJsonAsync<List<CampResponseModel>>();
            return Json(camps ??new List<CampResponseModel>());
        }


        [HttpGet]
        public async Task<IActionResult> GetWorkersByCamp(int campId)
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

            var response =
                await _httpClient.GetAsync($"api/Worker/by-camp/{campId}");

            if (!response.IsSuccessStatusCode)
            {
                return Json(new List<WorkerResponseModel>());
            }

            var workers =await response.Content.ReadFromJsonAsync<List<WorkerResponseModel>>();
            return Json(workers ??new List<WorkerResponseModel>());
        }

        [HttpGet]
        public async Task<IActionResult> AttendanceList(DateOnly? atDate,int? campId,string searchType = "camp",int? workerId = null)
        {
            if (!SetAuthorizationHeader())
            {
                return RedirectToAction("Login","Auth");
            }
            var role = HttpContext.Session.GetString("role");
            if (role != "Admin" && role != "Supervisor")
            {
                return Forbid();
            }

            if (!atDate.HasValue)
            {
                atDate =DateOnly.FromDateTime(DateTime.Today);
            }

            ViewBag.SelectedDate =atDate.Value.ToString("yyyy-MM-dd");
            ViewBag.SelectedCampId =campId?.ToString() ?? "";
            ViewBag.SearchType =searchType;
      
            var campResponse =await _httpClient.GetAsync("api/Camp/CampList");
            if (campResponse.IsSuccessStatusCode)
            {var camps =await campResponse.Content.ReadFromJsonAsync<List<CampResponseModel>>();
                ViewBag.Camps =camps ??new List<CampResponseModel>();
            }
            else
            {
                ViewBag.Camps =new List<CampResponseModel>();
            }

            if (searchType == "worker")
            {
                if (!workerId.HasValue ||workerId.Value <= 0)
                {
                    return View(new List<AttendanceResponseModel>());
                }
                var response =await _httpClient.GetAsync($"api/Attendance/by-worker/{workerId.Value}");
                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Error =response.StatusCode == System.Net.HttpStatusCode.Forbidden
                            ? "You can only view attendance for workers in your assigned camp."
                            : await response.Content.ReadAsStringAsync();

                    return View(new List<AttendanceResponseModel>());
                }
                var attendance =await response.Content.ReadFromJsonAsync<List<AttendanceResponseModel>>()?? new List<AttendanceResponseModel>();
                 return View(attendance);
            }


            if (role.Equals("Supervisor", StringComparison.OrdinalIgnoreCase) &&campId.HasValue &&campId.Value > 0)
            {
                var allowedCamp =(ViewBag.Camps as List<CampResponseModel>)?.Any(x => x.Id == campId.Value) == true;
                if (!allowedCamp)
                {
                    ViewBag.Error ="You can only view attendance for your assigned camp.";

                    return View(new List<AttendanceResponseModel>());
                }
            }

            var attendanceResponse =await _httpClient.GetAsync($"api/Attendance/by-date/{atDate.Value:yyyy-MM-dd}");
            if (!attendanceResponse.IsSuccessStatusCode)
            {
                ViewBag.Error ="Unable to load attendance records.";
                return View(new List<AttendanceResponseModel>());
            }
            var campAttendance =await attendanceResponse.Content.ReadFromJsonAsync<List<AttendanceResponseModel>>()?? new List<AttendanceResponseModel>();

            if (campId.HasValue &&campId.Value > 0)
            {
                var workerResponse =await _httpClient.GetAsync($"api/Worker/by-camp/{campId.Value}");
                if (workerResponse.IsSuccessStatusCode)
                {
                    var workers =await workerResponse.Content.ReadFromJsonAsync<List<WorkerResponseModel>>()?? new List<WorkerResponseModel>();
                    var workerIds =workers.Select(x => x.Id).ToList();
                    campAttendance =campAttendance.Where(x =>workerIds.Contains(x.WorkerId)).ToList();
                }
                else
                {
                    campAttendance =new List<AttendanceResponseModel>();
                }
            }
            return View(campAttendance);
        }


        [HttpGet]
        public IActionResult MarkAttendance()
        {
            if (!SetAuthorizationHeader())
            {
                return RedirectToAction("Login","Auth");
            }

            var role = HttpContext.Session.GetString("role");
            if (role != "Admin" && role != "Supervisor")
            {
                return Forbid();
            }

            return View(
                new BulkAttendanceViewModel
                {
                    AtDate =DateOnly.FromDateTime(DateTime.Today)
                }
            );
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAttendance(
            BulkAttendanceViewModel model)
        {
            if (!SetAuthorizationHeader())
            {
                return RedirectToAction("Login","Auth");
            }

            var role = HttpContext.Session.GetString("role");
            if (role != "Admin" && role != "Supervisor")
            {
                return Forbid();
            }

            if (model.Attendances == null ||
                !model.Attendances.Any())
            {
                ModelState.AddModelError("","No worker attendance selected.");

                return View(model);
            }

            var errors =new List<string>();

            foreach (var attendance in model.Attendances)
            {
                var attendanceModel =new AttendanceCreateModel
                    {
                        WorkerId =attendance.WorkerId,
                        AtDate =model.AtDate,
                        Status =attendance.Status
                    };

                var response =
                    await _httpClient.PostAsJsonAsync("api/Attendance",attendanceModel);
                if (!response.IsSuccessStatusCode)
                {
                    var error =await response.Content.ReadAsStringAsync();
                    errors.Add($"Worker ID {attendance.WorkerId}: {error}");
                }
            }


            if (errors.Any())
            {
                TempData["Error"] =string.Join(" | ",errors);
            }
            else
            {
                TempData["Success"] ="Attendance successfully marked for all workers.";
            }


            return RedirectToAction(nameof(AttendanceList),new{atDate =model.AtDate.ToString("yyyy-MM-dd"),campId =model.CampId});
        }

        [HttpGet]
        public async Task<IActionResult> EditAttendance(int id)
        {
            if (!SetAuthorizationHeader())
            {
                return RedirectToAction("Login","Auth");
            }

            var role = HttpContext.Session.GetString("role");
            if (role != "Admin" && role != "Supervisor")
            {
                return Forbid();
            }
            var response =await _httpClient.GetAsync($"api/Attendance/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] ="Attendance record not found.";
                return RedirectToAction(nameof(AttendanceList));
            }

            var attendance =await response.Content.ReadFromJsonAsync<AttendanceResponseModel>();
            if (attendance == null)
            {
                TempData["Error"] ="Attendance record not found.";
                return RedirectToAction(nameof(AttendanceList));
            }

            ViewBag.WorkerName =attendance.WorkerName;
            ViewBag.AttendanceDate =attendance.AtDate.ToString("yyyy-MM-dd");
            ViewBag.AttendanceId =attendance.Id;
            return View(new AttendanceUpdateModel
                {
                    Status =attendance.Status
                }
            );
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAttendance(int id,string atDate,AttendanceUpdateModel model)
        {
            if (!SetAuthorizationHeader())
            {
                return RedirectToAction("Login","Auth");
            }
            var role = HttpContext.Session.GetString("role");
            if (role != "Admin" && role != "Supervisor")
            {
                return Forbid();
            }
            var response =
                await _httpClient.PutAsJsonAsync($"api/Attendance/{id}",model);
            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] ="Attendance updated successfully.";
                return RedirectToAction(nameof(AttendanceList),new{atDate});
            }

            TempData["Error"] =await response.Content.ReadAsStringAsync();
            return RedirectToAction(nameof(EditAttendance),new{id});
        }

        [HttpGet]
        public async Task<IActionResult> AttendanceSummary(int workerId)
        {
            if (!SetAuthorizationHeader())
            {
                return RedirectToAction("Login","Auth");
            }

            var role = HttpContext.Session.GetString("role");
            if (role != "Admin" && role != "Supervisor")
            {
                return Forbid();
            }

            var response =
                await _httpClient.GetAsync($"api/Attendance/summary/{workerId}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] ="Unable to load attendance summary.";

                return RedirectToAction(nameof(AttendanceList));
            }

            var summary =await response.Content.ReadFromJsonAsync<AttendanceSummaryModel>();
            if (summary == null)
            {
                TempData["Error"] ="Attendance summary not found.";
                return RedirectToAction(nameof(AttendanceList));
            }
            var workerResponse =await _httpClient.GetAsync($"api/Worker/{workerId}");
            if (workerResponse.IsSuccessStatusCode)
            {
                var worker =await workerResponse.Content.ReadFromJsonAsync<WorkerResponseModel>();
                ViewBag.WorkerName =worker?.Name;
            }
            return View(summary);
        }

        [HttpGet]
        public async Task<IActionResult> MyAttendance()
        {
            if (!SetAuthorizationHeader())
            {
                return RedirectToAction("Login","Auth");
            }

            var role = HttpContext.Session.GetString("role");
            if (role != "Worker" && role != "Driver")
            {
                return Forbid();
            }

            var response =await _httpClient.GetAsync("api/Attendance/my-attendance");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] =await response.Content.ReadAsStringAsync();
                return View(new List<AttendanceResponseModel>());
            }

            var attendance =await response.Content.ReadFromJsonAsync<List<AttendanceResponseModel>>();
            return View(attendance ??new List<AttendanceResponseModel>()
            );
        }
    }
}