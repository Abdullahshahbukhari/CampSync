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
    public class BusController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BusController(ApplicationDbContext context) => _context = context;

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateBus([FromBody] BusCreateDTO model)
        {
            if (model.BusNo <= 0 || model.Capacity <= 0)
                return BadRequest("Bus number and capacity must be greater than zero.");

            var driver = await _context.Users.FirstOrDefaultAsync(x => x.Id == model.DriverId);
            if (driver == null)
                return NotFound("Driver does not exist.");
            if (driver.Role != "Driver")
                return BadRequest("Selected user is not registered as a Driver.");

            if (await _context.Buses.AnyAsync(x => x.DriverId == model.DriverId))
                return BadRequest("This driver is already assigned to another bus.");

            if (await _context.Buses.AnyAsync(x => x.BusNo == model.BusNo))
                return BadRequest("A bus with this BusNo already exists.");

            var bus = new Bus
            {
                BusNo = model.BusNo,
                DriverId = model.DriverId,
                Capacity = model.Capacity
            };

            _context.Buses.Add(bus);
            await _context.SaveChangesAsync();
            return Ok("Bus created successfully");
        }

        [Authorize(Roles = "Driver,Worker")]
        [HttpGet("my-bus")]
        public async Task<IActionResult> GetMyBus()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("User ID not found in token.");
            }
            var role = User.FindFirstValue(ClaimTypes.Role);
            var bus = await _context.Buses.Where(x => role == "Driver"? x.DriverId == userId : x.Workers.Any(w => w.UserId == userId))
                .Select(x => new BusResponseDTO
                {
                    Id = x.Id,
                    BusNo = x.BusNo,
                    DriverId = x.DriverId,
                    DriverName = x.Driver.Name,
                    Capacity = x.Capacity,
                    TotalWorkersAssigned = x.Workers.Count(),
                    Route = null,
                    RouteStops = x.Route_Stop.OrderBy(r => r.SeqOerder).Select(r => new RouteStopResponseDTO
                    {
                        Id = r.Id,
                        StopName = r.Name,
                        SeqOrder = r.SeqOerder,
                        BusId = r.BusId,
                        BusNo = r.Bus.BusNo
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            return bus == null ? NotFound("No bus is assigned to this user.") : Ok(bus);
        }

        [Authorize(Roles = "Admin,Supervisor")]
        [HttpGet]
        public async Task<IActionResult> GetAllBuses()
        {
            var buses = await _context.Buses.Select(x => new BusResponseDTO
            {
                Id = x.Id,
                BusNo = x.BusNo,
                DriverId = x.DriverId,
                DriverName = x.Driver.Name,
                Capacity = x.Capacity,
                TotalWorkersAssigned = x.Workers.Count()
            }).ToListAsync();
            return Ok(buses);
        }

        [Authorize(Roles = "Admin,Supervisor")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBusById(int id)
        {
            var bus = await _context.Buses.Where(x => x.Id == id).Select(x => new BusResponseDTO
            {
                Id = x.Id,
                BusNo = x.BusNo,
                DriverId = x.DriverId,
                DriverName = x.Driver.Name,
                Capacity = x.Capacity,
                TotalWorkersAssigned = x.Workers.Count()
            }).FirstOrDefaultAsync();

            return bus == null ? NotFound("Bus does not exist") : Ok(bus);
        }

        [Authorize(Roles = "Admin,Supervisor")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBus(int id, [FromBody] BusUpdateDTO model)
        {
            if (model.BusNo <= 0 || model.Capacity <= 0)
                return BadRequest("Bus number and capacity must be greater than zero.");

            var bus = await _context.Buses.FirstOrDefaultAsync(x => x.Id == id);
            if (bus == null)
            {
                return NotFound("Bus not found");
            }
            if (await _context.Buses.AnyAsync(x => x.BusNo == model.BusNo && x.Id != id))
            {
                return BadRequest("A bus with this BusNo already exists.");
            }
            var driver = await _context.Users.FirstOrDefaultAsync(x => x.Id == model.DriverId);
            if (driver == null)
            {
                return NotFound("Driver does not exist");
            }
            if (driver.Role != "Driver")
                return BadRequest("Selected user is not registered as a Driver");

            if (model.DriverId != bus.DriverId && await _context.Buses.AnyAsync(x => x.DriverId == model.DriverId && x.Id != id))
            {
                return BadRequest("This driver is already assigned to another bus.");
            }
            var assignedWorkers = await _context.Workers.CountAsync(x => x.BusId == id);
            if (model.Capacity < assignedWorkers)
            {
                return BadRequest($"Capacity cannot be lower than the {assignedWorkers} workers already assigned to this bus.");
            }
            bus.BusNo = model.BusNo;
            bus.DriverId = model.DriverId;
            bus.Capacity = model.Capacity;
            await _context.SaveChangesAsync();
            return Ok("Bus updated successfully");
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBus(int id)
        {
            var bus = await _context.Buses.FirstOrDefaultAsync(x => x.Id == id);
            if (bus == null)
                return NotFound("Bus does not exist");

            if (await _context.Workers.AnyAsync(x => x.BusId == id))
                return BadRequest("This bus has workers assigned. Reassign them before deleting.");

            _context.Buses.Remove(bus);
            await _context.SaveChangesAsync();
            return Ok("Bus deleted successfully");
        }
    }
}
