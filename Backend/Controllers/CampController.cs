using Backend.Data;
using Backend.DTO;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CampController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CampController(ApplicationDbContext context)
        {
            _context = context;
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateCamp([FromBody] CampDTO model)
        {
            if (model.SupervisorUserId <= 0)
            {
                return BadRequest("Please select a Supervisor.");
            }
            var campExists = await _context.Camps.AnyAsync(x => x.Name == model.Name);

            if (campExists)
            {
                return BadRequest("Camp already exist with this name");
            }
            var supervisorUser = await _context.Users.FirstOrDefaultAsync(x =>x.Id == model.SupervisorUserId && x.Role == "Supervisor");

            if (supervisorUser == null)
            {
                return BadRequest("Selected user is not a valid Supervisor.");
            }
            var alreadyAssigned = await _context.Supervisors.AnyAsync(x => x.UserId == model.SupervisorUserId);

            if (alreadyAssigned)
            {
                return BadRequest("This Supervisor is already assigned to a camp.");
            }
            var camp = new Camp
            {
                Name = model.Name,
                Location = model.Location
            };

            _context.Camps.Add(camp);
            await _context.SaveChangesAsync();

            var supervisor = new Supervisor
            {
                UserId = supervisorUser.Id,
                CampId = camp.Id
            };

            _context.Supervisors.Add(supervisor);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Camp created and Supervisor assigned successfully.",
                campId = camp.Id,
                supervisor = supervisorUser.Name
            });
        }
        [Authorize(Roles = "Admin,Supervisor")]
        [HttpGet("CampList")]
        public async Task<IActionResult> CampList()
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                var camps = await _context.Camps.OrderBy(c => c.Name)
                    .Select(c => new CampResponseDTO
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Location = c.Location,
                        TotalRooms = c.Rooms.Count,
                        TotalWorkers = c.Workers.Count
                    })
                    .ToListAsync();

                return Ok(camps);
            }

            if (string.Equals(role, "Supervisor", StringComparison.OrdinalIgnoreCase))
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!int.TryParse(userIdString, out int userId))
                    return Unauthorized();

                var camps = await _context.Supervisors.Where(s => s.UserId == userId)
                    .Select(s => new CampResponseDTO
                    {
                        Id = s.Camp.Id,
                        Name = s.Camp.Name,
                        Location = s.Camp.Location,
                        TotalRooms = s.Camp.Rooms.Count,
                        TotalWorkers = s.Camp.Workers.Count
                    })
                    .ToListAsync();

                return Ok(camps);
            }

            return Forbid();
        }


        [Authorize(Roles = "Admin,Supervisor")]
        [HttpGet("Campbyid/{Id}")]
        public async Task<IActionResult> Campbyid(int Id)
        {

            var CampList = await _context.Camps.Where(x => x.Id == Id).Include(x => x.Rooms).Include(x => x.Workers)
                .Select(x => new CampResponseDTO
                {
                    Id = x.Id,
                    Name = x.Name,
                    Location = x.Location,
                    TotalRooms = x.Rooms.Count,
                    TotalWorkers = x.Workers.Count

                })
                .FirstOrDefaultAsync();
            if (CampList == null)
            {
                return NotFound("Camp does not exist");
            }

            return Ok(CampList);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("Campbyid/{Id}")]
        public async Task<IActionResult> editcamp(int id, [FromBody] CampUpdateDTO model)
        {
            var camp = await _context.Camps.Where(x => x.Id == id).FirstOrDefaultAsync();
            if (camp == null)
            {
                return BadRequest("No camp with this id");
            }

            camp.Name = model.Name;
            camp.Location = model.Location;

            _context.Camps.Update(camp);
            await _context.SaveChangesAsync();
            return Ok("Camp updated successfully");
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("Dlete/{id}")]
        public async Task<IActionResult> delete(int id)
        {
            var camp = await _context.Camps.FirstOrDefaultAsync(x => x.Id == id);
            if (camp == null)
            {
                return BadRequest("No camp with this id");
            }
            var room = await _context.Rooms.FirstOrDefaultAsync(x => x.CampId == id);
            if (room != null)
            {
                return BadRequest("Camp has active rooms");
            }
            var worker = await _context.Workers.FirstOrDefaultAsync(x => x.CampId == id);
            if (worker != null)
            {
                return BadRequest("Camp has active workerss");
            }
            var supervisors = await _context.Supervisors.Where(s => s.CampId == id).ToListAsync();

            if (supervisors.Any())
            {
                _context.Supervisors.RemoveRange(supervisors);
            }
            _context.Camps.Remove(camp);
            await _context.SaveChangesAsync();
            return Ok("Camp deleted successfully");
        }
    }
}
