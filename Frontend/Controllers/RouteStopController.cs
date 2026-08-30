
using Frontend.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Frontend.Controllers
{
    public class RouteStopController : Controller
    {
        private readonly HttpClient _httpClient;
        public RouteStopController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress =new Uri("https://localhost:7232/");
        }
        private bool SetAuthorizationHeader()
        {
            var token =HttpContext.Session.GetString("token");

            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            _httpClient.DefaultRequestHeaders.Authorization =new AuthenticationHeaderValue("Bearer",token);
            return true;
        }
        private string GetRole()
        {
            return HttpContext.Session.GetString("role") ?? "";
        }
        private bool IsAdmin()
        {
            return GetRole() == "Admin";
        }
        private bool IsAdminOrSupervisor()
        {
            var role = GetRole();
            return role == "Admin" ||role == "Supervisor";
        }
        private bool CanViewRouteStops()
        {
            var role = GetRole();
            return role == "Admin" ||role == "Supervisor" ||role == "Driver";
        }
 
        [HttpGet]
        public async Task<IActionResult> RouteStopList(
            int? busId)
        {
            if (!SetAuthorizationHeader())
            {
                return RedirectToAction( "Login",  "Auth");
            }
            if (!CanViewRouteStops())
            {
                return Forbid();
            }

            ViewBag.SelectedBusId = busId;

            if (!busId.HasValue)
            {
                return View(new List<RouteStopResponseModel>());
            }

            var response =await _httpClient.GetAsync($"api/RouteStop/by-bus/{busId.Value}");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error =await response.Content.ReadAsStringAsync();
                return View(new List<RouteStopResponseModel>());
            }
            var stops =await response.Content.ReadFromJsonAsync<List<RouteStopResponseModel>>();
            return View(stops ??new List<RouteStopResponseModel>());
        }


        [HttpGet]
        public async Task<IActionResult> GetBuses()
        {
            if (!SetAuthorizationHeader())
            {
                return Unauthorized();
            }

            if (!IsAdminOrSupervisor())
            {
                return Forbid();
            }

            var response =
                await _httpClient.GetAsync("api/Bus");

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode(
                    (int)response.StatusCode
                );
            }

            var buses =await response.Content.ReadFromJsonAsync<List<BusResponseModel>>();
            return Json(buses ??new List<BusResponseModel>());
        }


        [HttpGet]
        public IActionResult CreateRouteStop()
        {
            if (!SetAuthorizationHeader())
            {
                return RedirectToAction("Login","Auth");
            }

            if (!IsAdmin())
            {
                return Forbid();
            }
            var model =new RouteStopCreatePageModel();
            model.Stops.Add(new RouteStopCreateModel());
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRouteStop(
            RouteStopCreatePageModel model)
        {
            if (!SetAuthorizationHeader())
            {
                return RedirectToAction(
                    "Login",
                    "Auth"
                );
            }

            if (!IsAdmin())
            {
                return Forbid();
            }

            if (model.BusId <= 0)
            {
                ModelState.AddModelError("","Please select a bus.");
                return View(model);
            }

            if (model.Stops == null ||!model.Stops.Any())
            {
                ModelState.AddModelError("","Please add at least one route stop.");
                return View(model);
            }

            var validStops =model.Stops.Where(x =>!string.IsNullOrWhiteSpace(x.StopName)&&x.SeqOrder > 0).ToList();
            if (!validStops.Any())
            {
                ModelState.AddModelError("","Please enter valid stop names and sequence orders.");
                return View(model);
            }

            var duplicateSequence =validStops.GroupBy(x => x.SeqOrder).Any(x => x.Count() > 1);
            if (duplicateSequence)
            {
                ModelState.AddModelError("","Duplicate sequence order is not allowed.");
                return View(model);
            }

            var bulkModel =new
                {
                    BusId = model.BusId,
                    Stops = validStops
                        .Select(x => new
                        {
                            BusId = model.BusId,
                            StopName = x.StopName,
                            SeqOrder = x.SeqOrder
                        })
                        .ToList()
                };
            var response =await _httpClient.PostAsJsonAsync("api/RouteStop/bulk-create",bulkModel);
            if (!response.IsSuccessStatusCode)
            {
                var error =await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("",string.IsNullOrWhiteSpace(error)? "Unable to create route stops.": error);
                return View(model);
            }
            TempData["Success"] ="Route stops created successfully.";
            return RedirectToAction(nameof(RouteStopList),new
                {
                    busId = model.BusId
                }
            );
        }


        [HttpGet]
        public async Task<IActionResult> EditRouteStop(
            int id)
        {
            if (!SetAuthorizationHeader())
            {
                return RedirectToAction("Login","Auth");
            }
            if (!IsAdminOrSupervisor())
            {
                return Forbid();
            }

            var response =
                await _httpClient.GetAsync($"api/RouteStop/{id}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] =await response.Content.ReadAsStringAsync();
                return RedirectToAction(nameof(RouteStopList));
            }
            var stop =await response.Content.ReadFromJsonAsync<RouteStopResponseModel>();
            if (stop == null)
            {
                TempData["Error"] ="Route stop does not exist.";
                return RedirectToAction(nameof(RouteStopList));
            }
            return View(stop);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRouteStop(int id,RouteStopUpdateModel model,int busId)
        {
            if (!SetAuthorizationHeader())
            {
                return RedirectToAction("Login","Auth");
            }
            if (!IsAdminOrSupervisor())
            {
                return Forbid();
            }
            if (!ModelState.IsValid)
            {
                return View("EditRouteStop",
                    new RouteStopResponseModel
                    {
                        Id = id,
                        StopName = model.StopName,
                        SeqOrder = model.SeqOrder,
                        BusId = busId
                    }
                );
            }

            var response =await _httpClient.PutAsJsonAsync($"api/RouteStop/{id}",model);
            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] ="Route stop updated successfully.";
                return RedirectToAction(nameof(RouteStopList),new{busId});
            }
            TempData["Error"] =await response.Content.ReadAsStringAsync();
            return RedirectToAction(nameof(EditRouteStop),new{id});
        }

        [HttpGet]
        public async Task<IActionResult> DeleteRouteStop(
            int id,
            int? busId)
        {
            if (!SetAuthorizationHeader())
            {
                return RedirectToAction("Login","Auth");
            }

            if (!IsAdmin())
            {
                return Forbid();
            }

            var response =await _httpClient.GetAsync($"api/RouteStop/{id}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] =await response.Content.ReadAsStringAsync();
                return RedirectToAction(nameof(RouteStopList),new { busId });
            }
            var stop =await response.Content.ReadFromJsonAsync<RouteStopResponseModel>();
            if (stop == null)
            {
                TempData["Error"] ="Route stop does not exist.";
                return RedirectToAction(nameof(RouteStopList),new { busId });
            }
            ViewBag.BusId =busId ?? stop.BusId;
            return View(stop);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRouteStop(
            int id,
            int busId)
        {
            if (!SetAuthorizationHeader())
            {
                return RedirectToAction(
                    "Login",
                    "Auth"
                );
            }

            if (!IsAdmin())
            {
                return Forbid();
            }
            var response =await _httpClient.DeleteAsync($"api/RouteStop/{id}");
            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] ="Route stop deleted successfully.";
            }
            else
            {
                TempData["Error"] =await response.Content.ReadAsStringAsync();
            }
            return RedirectToAction(nameof(RouteStopList),new{busId});
        }
    }
}

