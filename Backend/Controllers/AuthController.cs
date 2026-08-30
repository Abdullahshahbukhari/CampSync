using Backend.Data;
using Backend.DTO;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var anyUserExists = await _context.Users.AnyAsync();

            if (anyUserExists)
            {
                return Conflict(new
                {
                    message = "Registration is no longer available. An administrator account already exists. Please contact the system administrator."
                });
            }

            var user = new User
            {
                Name = model.Name,
                Email = model.Email,
                Passwordhash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = "Admin"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok(new
            {
                message = "Administrator account created successfully."
            });
        }

        [HttpPost("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDTO model)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(x => x.Email == model.Email);

            if (existingUser == null)
                return BadRequest("User does not exist.");

            if (!BCrypt.Net.BCrypt.Verify(model.Password, existingUser.Passwordhash))
                return Unauthorized("Invalid password or email.");

            var token = GenerateJwtToken(existingUser);
            return Ok(new { Token = token, Message = "User Login successfully" });
        }
        [HttpPost("register-by-admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RegisterByAdmin( RegisterDTO model)
        {
            var allowedRoles = new[] {
        "Worker",
        "Supervisor",
        "Driver"
                                      };

            if (!allowedRoles.Contains(model.Role,StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest("Invalid role.");
            }

            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                return BadRequest( "User with this email already exists.");
            }

            var user = new User
            {
                Name = model.Name,
                Email = model.Email,
                Passwordhash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = model.Role
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok(new
            {
                message = $"{model.Role} created successfully."
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("Workers")]
        public async Task<IActionResult> GetWorkers()
        {
            var workers = await _context.Users
                .Where(x => x.Role == "Worker")
                .Where(x => !_context.Workers.Any(w => w.UserId == x.Id))
                .Select(x => new { Id = x.Id, Name = x.Name, Email = x.Email })
                .ToListAsync();

            return Ok(workers);
        }

        [Authorize(Roles = "Admin,Supervisor")]
        [HttpGet("Drivers")]
        public async Task<IActionResult> GetDrivers()
        {
            var drivers = await _context.Users
                .Where(x => x.Role == "Driver")
                .Select(x => new { Id = x.Id, Name = x.Name })
                .ToListAsync();

            return Ok(drivers);
        }

        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.Name, user.Name)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(double.Parse(_configuration["JwtSettings:ExpiryInDays"]!)),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
