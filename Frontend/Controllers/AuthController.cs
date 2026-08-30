using Frontend.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Net.Http.Json;

namespace Frontend.Controllers
{
    public class AuthController : Controller
    {
        private readonly HttpClient _httpClient;

        public AuthController(IHttpClientFactory httpClientFactory)
        {
            _httpClient =httpClientFactory.CreateClient();
            _httpClient.BaseAddress =new Uri("https://localhost:7232/");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var response =await _httpClient.PostAsJsonAsync("api/Auth/Login",model);

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error =await response.Content.ReadAsStringAsync();
                return View(model);
            }

            var result =await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (result == null ||string.IsNullOrEmpty(result.token))
            {
                ViewBag.Error ="Login failed.";
                return View(model);
            }

            var handler =new JwtSecurityTokenHandler();
            var jwtToken =handler.ReadJwtToken(result.token);
            var role =jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value;
            var name =jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
            var userId =jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            var email =jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;

            // SAVE JWT + USER DATA IN SESSION
            HttpContext.Session.SetString("token",result.token);
            HttpContext.Session.SetString("role",role ?? "");
            HttpContext.Session.SetString("name",name ?? "");
            HttpContext.Session.SetString("userID",userId ?? "");
            HttpContext.Session.SetString("email",email ?? "");

            // COOKIE AUTHENTICATION
            var claims =jwtToken.Claims.ToList();
            var identity =new ClaimsIdentity(claims,CookieAuthenticationDefaults.AuthenticationScheme);
            var principal =new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,principal);
            return RedirectToAction("Welcome", "Home");
        }


        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var response = await _httpClient.PostAsJsonAsync("api/Auth/register",model);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();

                try
                {
                    var error = System.Text.Json.JsonSerializer
                        .Deserialize<Dictionary<string, string>>(errorBody);

                    ViewBag.Error =error?.GetValueOrDefault("message")?? "Registration is no longer available.";
                }
                catch
                {
                    ViewBag.Error ="Registration is no longer available.";
                }

                return View(model);
            }

            TempData["Success"] ="Administrator account created successfully.";

            return RedirectToAction("Login","Auth");
        }

        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}