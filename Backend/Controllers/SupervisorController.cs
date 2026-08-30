using Backend.Data;
using Backend.DTO;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupervisorController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SupervisorController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("Available")]
        public async Task<IActionResult> GetAvailableSupervisors()
        {
            var supervisors = await _context.Users.Where(x =>x.Role == "Supervisor" &&!_context.Supervisors.Any(s => s.UserId == x.Id)).OrderBy(x => x.Name).Select(x => new
                {
                    UserId = x.Id,
                    Name = x.Name,
                    Email = x.Email
                })
                .ToListAsync();

            return Ok(supervisors);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("AssignToCamp")]
        public async Task<IActionResult> AssignToCamp([FromBody] SupervisorCreateDTO model)
        {

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == model.UserId);
            if (user == null)
            {
                return BadRequest("Selected user does not exist.");
            }

            if (!string.Equals(user.Role,"Supervisor",StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Selected user is not a Supervisor.");
            }

            var camp = await _context.Camps.FirstOrDefaultAsync(x => x.Id == model.CampId);

            if (camp == null)
            {
                return BadRequest("Selected camp does not exist.");
            }

            var existingSupervisor =await _context.Supervisors.FirstOrDefaultAsync(x => x.UserId == model.UserId);
            if (existingSupervisor != null)
            {
                return BadRequest("This Supervisor is already assigned to a camp.");
            }

            var supervisor = new Supervisor
            {
                UserId = model.UserId,
                CampId = model.CampId
            };

            _context.Supervisors.Add(supervisor);
            await _context.SaveChangesAsync();
            return Ok(new
            {message ="Supervisor assigned to camp successfully."
            });
        }


        [Authorize(Roles = "Admin")]
        [HttpGet("List")]
        public async Task<IActionResult> List()
        {
            var supervisors = await _context.Supervisors.Include(x => x.User).Include(x => x.Camp)

                .Select(x => new SupervisorResponseDTO
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    SupervisorName = x.User.Name,
                    Email = x.User.Email,
                    CampId = x.CampId,
                    CampName = x.Camp.Name,
                    CampLocation = x.Camp.Location
                })
                .ToListAsync();

            return Ok(supervisors);
        }

        [Authorize(Roles = "Supervisor")]
        [HttpGet("MyCamp")]
        public async Task<IActionResult> MyCamp()
        {
            var userIdString =User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString,out int userId))
            {
                return Unauthorized();
            }
            var supervisor =await _context.Supervisors.Include(x => x.User).Include(x => x.Camp).ThenInclude(c => c.Rooms).ThenInclude(r => r.Beds).FirstOrDefaultAsync(x => x.UserId == userId);
            if (supervisor == null)
            {
                return NotFound("You have not been assigned to any camp yet.");
            }
            var result = new
            {
                Supervisor = new
                {
                    Id = supervisor.User.Id,
                    Name = supervisor.User.Name,
                    Email = supervisor.User.Email,
                    Role = supervisor.User.Role
                },

                Camp = new
                {
                    Id = supervisor.Camp.Id,
                    Name = supervisor.Camp.Name,
                    Location = supervisor.Camp.Location,
                    TotalRooms = supervisor.Camp.Rooms.Count,
                    TotalBeds =supervisor.Camp.Rooms.SelectMany(x => x.Beds).Count(),
                    OccupiedBeds =supervisor.Camp.Rooms.SelectMany(x => x.Beds).Count(x => x.Isoccupied)
                }
            };

            return Ok(result);
        }


        [Authorize(Roles = "Supervisor")]
        [HttpGet("MyWorkers")]
        public async Task<IActionResult> MyWorkers()
        {
            var userIdString =User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString,out int userId))
            {
                return Unauthorized();
            }
            var supervisor =await _context.Supervisors.FirstOrDefaultAsync(x => x.UserId == userId);
            if (supervisor == null)
            {
                return NotFound("You have not been assigned to any camp yet.");
            }


            var workers = await _context.Workers.Where(x =>x.CampId == supervisor.CampId).Include(x => x.Camp).Include(x => x.Bed).ThenInclude(x => x.Room).Include(x => x.Bus)
            .Select(x => new
                {
                    Id = x.Id,
                    Name = x.Name,
                    IqamaNo = x.IqamaNo,
                    Nationality = x.Nationality,
                    Trade = x.Trade,
                    CampId = x.CampId,
                    CampName = x.Camp.Name,
                    RoomId = x.Bed.RoomId,
                    RoomNo = x.Bed.Room.RoomNo,
                    BedId = x.BedId,
                    BedNo = x.Bed.BedNo,
                    BusId = x.BusId,
                    BusNo = x.Bus.BusNo
                }).ToListAsync();

            return Ok(workers);
        }


        [Authorize(Roles = "Supervisor")]
        [HttpGet("MyCampAttendance")]
        public async Task<IActionResult> MyCampAttendance()
        {
            var userIdString =User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString,out int userId))
            {
                return Unauthorized();
            }
            var supervisor =await _context.Supervisors.FirstOrDefaultAsync(x => x.UserId == userId);

            if (supervisor == null)
            {
                return NotFound("You have not been assigned to any camp yet.");
            }


            var attendance =await _context.Attendances.Include(x => x.Worker).Where(x =>x.Worker.CampId ==supervisor.CampId).OrderByDescending(x => x.AtDate)
                        .Select(x => new
                    {
                        x.Id,
                        x.AtDate,
                        x.Status,
                        WorkerId = x.WorkerId,
                        WorkerName = x.Worker.Name,
                        IqamaNo = x.Worker.IqamaNo
                    }) .ToListAsync();

            return Ok(attendance);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("Unassign/{userId}")]
        public async Task<IActionResult> Unassign(int userId)
        {
            var supervisor =await _context.Supervisors.FirstOrDefaultAsync(x => x.UserId == userId);

            if (supervisor == null)
            {
                return NotFound("Supervisor assignment not found.");
            }

            _context.Supervisors.Remove(supervisor);
            await _context.SaveChangesAsync();
            return Ok("Supervisor unassigned successfully.");
        }
    }
}