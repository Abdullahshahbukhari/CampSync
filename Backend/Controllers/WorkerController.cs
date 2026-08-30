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
    public class WorkerController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public WorkerController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("CreateWorker")]
        public async Task<IActionResult> CreateWorker( [FromBody] WorkerDTO model)
        {
            var existingWorker = await _context.Workers.FirstOrDefaultAsync(x => x.IqamaNo == model.IqamaNo);

            if (existingWorker != null)
            {
                return BadRequest("Worker already registered with this IqamaNo");
            }
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == model.UserId);
            if (user == null)
            {
                return BadRequest("User with this ID does not exist.");
            }

            if (!string.Equals(user.Role,"Worker",StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Selected user is not registered as a Worker.");
            }

            var existingUserWorker = await _context.Workers.FirstOrDefaultAsync(x => x.UserId == model.UserId);

            if (existingUserWorker != null)
            {
                return BadRequest("This user already has a worker profile.");
            }

            var camp = await _context.Camps.FirstOrDefaultAsync(x => x.Id == model.CampId);
            if (camp == null)
            {
                return BadRequest("Camp with this ID does not exist");
            }

            var bus = await _context.Buses.FirstOrDefaultAsync(x => x.Id == model.BusId);
            if (bus == null)
            {
                return BadRequest("Bus with this ID does not exist");
            }

            var bed = await _context.Beds.Include(x => x.Room).FirstOrDefaultAsync(x => x.Id == model.BedId);
            if (bed == null)
            {
                return BadRequest("Bed with this ID does not exist");
            }

            if (bed.Room.CampId != model.CampId)
            {
                return BadRequest("Selected bed does not belong to the selected camp.");
            }
            if (bus.Capacity <= 0)
            {
                return BadRequest("Selected bus has no valid capacity. Set its capacity before assigning workers.");
            }

            var assignedWorkers =await _context.Workers.CountAsync(x => x.BusId == model.BusId);
            if (assignedWorkers >= bus.Capacity)
            {
                return BadRequest("Selected bus is already at full capacity.");
            }
            if (bed.Isoccupied)
            {
                return BadRequest("Bed already assigned to another worker");
            }

            var worker = new Worker
            {
                Name = model.Name,
                IqamaNo = model.IqamaNo,
                Nationality = model.Nationality,
                Trade = model.Trade,

                CampId = model.CampId,
                BedId = model.BedId,
                BusId = model.BusId,

                UserId = model.UserId
            };

            bed.Isoccupied = true;

            _context.Workers.Add(worker);
            _context.Beds.Update(bed);
            await _context.SaveChangesAsync();
            return Ok("Worker created successfully");
        }


        [Authorize(Roles = "Worker")]
        [HttpGet("my-profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userIdClaim =User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("User ID not found in token.");
            }

            var worker = await _context.Workers.Where(x => x.UserId == userId)
                .Select(x => new WorkerResponseDTO
                {
                    Id = x.Id,
                    Name = x.Name,
                    Nationality = x.Nationality,
                    IqamaNo = x.IqamaNo,
                    Trade = x.Trade,

                    CampId = x.CampId,
                    CampName = x.Camp.Name,

                    RoomId = x.Bed.RoomId,
                    RoomNo = x.Bed.Room.RoomNo,

                    BedId = x.BedId,
                    BedNo = x.Bed.BedNo,

                    BusId = x.BusId,
                    BusNo = x.Bus.BusNo
                })
                .FirstOrDefaultAsync();

            return worker == null? NotFound("Worker profile not found for this user."): Ok(worker);
        }


        [Authorize(Roles = "Admin,Supervisor")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetWorkerById(int id)
        {
            var role =User.FindFirst(ClaimTypes.Role)?.Value;
            var query = _context.Workers.Where(x => x.Id == id);


            if (string.Equals(role,"Supervisor",StringComparison.OrdinalIgnoreCase))
            {
                var supervisorCampId =await GetSupervisorCampId();
                if (supervisorCampId == null)
                {
                    return NotFound("You have not been assigned to any camp yet.");
                }
                query = query.Where(x => x.CampId == supervisorCampId.Value);
            }

            var worker = await query.Include(x => x.Bus).Include(x => x.Camp).Include(x => x.Bed).ThenInclude(x => x.Room)
                .Select(x => new WorkerResponseDTO
                {
                    Id = x.Id,
                    Name = x.Name,
                    Nationality = x.Nationality,
                    IqamaNo = x.IqamaNo,
                    Trade = x.Trade,

                    CampId = x.CampId,
                    CampName = x.Camp.Name,

                    RoomId = x.Bed.RoomId,
                    RoomNo = x.Bed.Room.RoomNo,

                    BedId = x.BedId,
                    BedNo = x.Bed.BedNo,

                    BusId = x.BusId,
                    BusNo = x.Bus.BusNo
                })
                .FirstOrDefaultAsync();
            if (worker == null)
            {
                return NotFound("Worker does not exist or is outside your assigned camp.");
            }

            return Ok(worker);
        }


        [Authorize(Roles = "Admin,Supervisor")]
        [HttpGet("by-camp/{campId}")]
        public async Task<IActionResult> GetWorkerList(int campId)
        {
            var role =User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.Equals(role,"Supervisor",StringComparison.OrdinalIgnoreCase))
            {
                var supervisorCampId =await GetSupervisorCampId();

                if (supervisorCampId == null)
                {
                    return NotFound("You have not been assigned to any camp yet.");
                }

                if (supervisorCampId.Value != campId)
                {
                    return Forbid();
                }
            }


            var workers = await _context.Workers.Where(x => x.CampId == campId).Include(x => x.Bus).Include(x => x.Camp).Include(x => x.Bed).ThenInclude(x => x.Room)
                .Select(x => new WorkerResponseDTO
                {
                    Id = x.Id,
                    Name = x.Name,
                    Nationality = x.Nationality,
                    IqamaNo = x.IqamaNo,
                    Trade = x.Trade,

                    CampId = x.CampId,
                    CampName = x.Camp.Name,

                    RoomId = x.Bed.RoomId,
                    RoomNo = x.Bed.Room.RoomNo,

                    BedId = x.BedId,
                    BedNo = x.Bed.BedNo,

                    BusId = x.BusId,
                    BusNo = x.Bus.BusNo
                })

                .ToListAsync();

            return Ok(workers);
        }


        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Bedupdate(
            int id,
            [FromBody] WorkerDTO model)
        {
            var worker = await _context.Workers.FirstOrDefaultAsync(x => x.Id == id);

            if (worker == null)
            {
                return NotFound("Worker does not exist.");
            }

            var camp = await _context.Camps.FirstOrDefaultAsync(x => x.Id == model.CampId);
            if (camp == null)
            {
                return NotFound("No camp with this Id.");
            }

            var bus = await _context.Buses.FirstOrDefaultAsync(x => x.Id == model.BusId);
            if (bus == null)
            {
                return NotFound("No bus with this Id.");
            }
            if (bus.Capacity <= 0)
            {
                return BadRequest("Selected bus has no valid capacity.");
            }

            var newBed = await _context.Beds.Include(x => x.Room).FirstOrDefaultAsync(x => x.Id == model.BedId);
            if (newBed == null)
            {
                return NotFound("No bed with this Id.");
            }

            if (newBed.Room.CampId != model.CampId)
            {
                return BadRequest("Selected bed does not belong to the selected camp.");
            }

            if (model.BedId != worker.BedId &&newBed.Isoccupied)
            {
                return BadRequest("Bed is already occupied.");
            }

            if (model.BusId != worker.BusId)
            {
                var assignedWorkers =await _context.Workers.CountAsync(x =>x.BusId == model.BusId &&x.Id != worker.Id);
                if (assignedWorkers >= bus.Capacity)
                {
                    return BadRequest("Selected bus is already at full capacity.");
                }
            }

            if (model.BedId != worker.BedId)
            {
                var oldBed =await _context.Beds.FirstOrDefaultAsync(x => x.Id == worker.BedId);
                if (oldBed != null)
                {
                    oldBed.Isoccupied = false;
                }

                newBed.Isoccupied = true;
                worker.BedId = model.BedId;
            }

            worker.CampId = model.CampId;
            worker.BusId = model.BusId;
            worker.Name = model.Name;
            worker.Nationality = model.Nationality;
            worker.Trade = model.Trade;

            await _context.SaveChangesAsync();

            return Ok("Worker updated successfully");
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Removeworker(int id)
        {
            var worker = await _context.Workers.FirstOrDefaultAsync(x => x.Id == id);
            if (worker == null)
            {
                return NotFound("This worker does not exist");
            }
            var bed = await _context.Beds.FirstOrDefaultAsync(x => x.Id == worker.BedId);
            if (bed != null)
            {
                bed.Isoccupied = false;
                _context.Beds.Update(bed);
            }


            var attendances =
                await _context.Attendances.Where(x => x.WorkerId == id).ToListAsync();

            if (attendances.Any())
            {
                _context.Attendances.RemoveRange(attendances);
            }
            _context.Workers.Remove(worker);
            await _context.SaveChangesAsync();
            return Ok("Worker deleted successfully");
        }


        private async Task<int?> GetSupervisorCampId()
        {
            var userIdClaim =User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim,out var userId))
            {
                return null;
            }

            var supervisor =await _context.Supervisors.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userId);
            return supervisor?.CampId;
        }
    }
}