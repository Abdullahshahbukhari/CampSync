using Backend.Data;
using Backend.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Supervisor,Worker,Driver")]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public UserController(ApplicationDbContext context) => _context = context;

        [HttpGet("my-profile")]
        public async Task<IActionResult> MyProfile()
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized("User ID not found in token.");

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null) return NotFound("User profile not found.");

            var result = new MyProfileResponseDTO
            {
                UserId = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role
            };

            result.Worker = await _context.Workers.Where(x => x.UserId == userId)
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
                }).FirstOrDefaultAsync();

            if (user.Role.Equals("Supervisor", StringComparison.OrdinalIgnoreCase))
            {
                result.Supervisor = await _context.Supervisors.Where(x => x.UserId == userId)
                    .Select(x => new SupervisorProfileDTO
                    {
                        Id = x.Id,
                        CampId = x.CampId,
                        CampName = x.Camp.Name,
                        CampLocation = x.Camp.Location,
                        TotalRooms = x.Camp.Rooms.Count,
                        TotalBeds = x.Camp.Rooms.SelectMany(r => r.Beds).Count(),
                        OccupiedBeds = x.Camp.Rooms.SelectMany(r => r.Beds).Count(b => b.Isoccupied)
                    })
                    .FirstOrDefaultAsync();
            }

            var bus = await _context.Buses.Where(x => x.DriverId == userId || x.Workers.Any(w => w.UserId == userId))
                .Select(x => new MyBusProfileDTO
                {
                    Id = x.Id,
                    BusNo = x.BusNo,
                    Capacity = x.Capacity,
                    TotalWorkersAssigned = x.Workers.Count(),
                    RouteStops = x.Route_Stop.OrderBy(r => r.SeqOerder).Select(r => new RouteStopResponseDTO
                    {
                        Id = r.Id,
                        StopName = r.Name,
                        SeqOrder = r.SeqOerder,
                        BusId = r.BusId,
                        BusNo = r.Bus.BusNo
                    }).ToList()
                }).FirstOrDefaultAsync();
            result.Bus = bus;

            if (result.Worker != null)
            {
                result.Attendance = await _context.Attendances.Where(x => x.Worker.UserId == userId).OrderByDescending(x => x.AtDate)
                    .Select(x => new AttendanceResponseDTO
                    {
                        Id = x.Id,
                        WorkerId = x.WorkerId,
                        WorkerName = x.Worker.Name,
                        AtDate = x.AtDate,
                        Status = x.Status,
                        MarkedBy = x.MarkedBy.Name
                    }).ToListAsync();
            }

            if (user.Role.Equals("Driver", StringComparison.OrdinalIgnoreCase))
            {
                result.DriverAttendance = await _context.DriverAttendances.Where(x => x.DriverId == userId).OrderByDescending(x => x.AtDate)
                    .Select(x => new DriverAttendanceProfileDTO
                    {
                        Id = x.Id,
                        AtDate = x.AtDate,
                        Status = x.Status,
                        MarkedBy = x.MarkedBy.Name
                    }).ToListAsync();
            }

            result.Leaves = await _context.Leaves.Where(x => x.UserId == userId).OrderByDescending(x => x.FromeDate)
                .Select(x => new LeaveResponseDTO
                {
                    Id = x.Id,
                    UserName = x.User.Name,
                    Role = x.User.Role,
                    FromDate = x.FromeDate,
                    ToDate = x.ToDate,
                    Reason = x.Reason,
                    Status = x.Status,
                    CampName = x.UserId == userId && x.User.Role == "Worker"
                        ? _context.Workers.Where(w => w.UserId == userId).Select(w => w.Camp.Name).FirstOrDefault(): null,
                    ApprovedByName = x.ApprovedByUser != null ? x.ApprovedByUser.Name : null,
                    RejectionReason = x.RejectionReason
                }).ToListAsync();

            return Ok(result);
        }
    }
}
