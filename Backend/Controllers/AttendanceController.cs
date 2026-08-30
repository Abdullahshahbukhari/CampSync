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
    public class AttendanceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AttendanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        

        [Authorize(Roles = "Admin,Supervisor")]
        [HttpPost]
        public async Task<IActionResult> MarkAttendance([FromBody] AttendanceCreateDTO model)
        {
            if (model.Status is not ("Present" or "Absent" or "Leave"))
            {
                return BadRequest(
                    "Status must be Present, Absent or Leave.");
            }

            var worker = await _context.Workers
                .FirstOrDefaultAsync(x => x.Id == model.workerId);

            if (worker == null)
            {
                return NotFound("Worker does not exist");
            }

           
            if (!await CanAccessWorker(worker.CampId))
            {
                return Forbid();
            }

            var attendanceDate = model.AtDate;

            if (attendanceDate >DateOnly.FromDateTime(DateTime.Today))
            {
                return BadRequest("Cannot mark attendance for a future date.");
            }

            var existingAttendance =await _context.Attendances.FirstOrDefaultAsync(x =>x.WorkerId == model.workerId &&x.AtDate == attendanceDate);
            if (existingAttendance != null)
            {
                return BadRequest("Attendance for this worker on this date is already marked.");
            }

            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier),out var userId))
            {
                return Unauthorized( "User ID not found in token.");
            }

            var attendance = new Attendance
            {
                WorkerId = model.workerId,
                AtDate = attendanceDate,
                Status = model.Status,
                UserId = userId
            };

            _context.Attendances.Add(attendance);

            await _context.SaveChangesAsync();

            return Ok("Attendance marked successfully.");
        }


        
        [Authorize(Roles = "Admin,Supervisor")]
        [HttpPost("driver")]
        public async Task<IActionResult> MarkDriverAttendance([FromBody] DriverAttendanceCreateDTO model)
        {
            if (model.AtDate > DateOnly.FromDateTime(DateTime.Today))
            {
                return BadRequest("Cannot mark attendance for a future date.");
            }

            if (model.Status is not ("Present" or "Absent" or "Leave"))
            {
                return BadRequest("Status must be Present, Absent or Leave.");
            }

            var driver = await _context.Users .FirstOrDefaultAsync(x =>x.Id == model.DriverId &&x.Role == "Driver");

            if (driver == null)
            {
                return NotFound("Driver does not exist.");
            }

          
            if (IsSupervisor())
            {
                var supervisorCampId =await GetSupervisorCampId();

                if (supervisorCampId == null)
                {
                    return NotFound("You have not been assigned to any camp yet.");
                }

                var driverHasBusInCamp = await _context.Workers.AnyAsync(x => x.CampId ==supervisorCampId.Value && x.Bus.DriverId == model.DriverId);

                if (!driverHasBusInCamp)
                {
                    return Forbid();
                }
            }

            if (await _context.DriverAttendances.AnyAsync( x =>x.DriverId == model.DriverId && x.AtDate == model.AtDate))
            {
                return BadRequest("Attendance for this driver on this date is already marked.");
            }

            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier),out var markedBy))
            {
                return Unauthorized("User ID not found in token.");
            }

            _context.DriverAttendances.Add(new DriverAttendance
                {
                    DriverId = model.DriverId,
                    AtDate = model.AtDate,
                    Status = model.Status,
                    MarkedByUserId = markedBy
                });

            await _context.SaveChangesAsync();

            return Ok( "Driver attendance marked successfully.");
        }


       
        [Authorize(Roles = "Admin,Supervisor")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAttendanceById(int id)
        {
            var attendance = await _context.Attendances
                    .Include(x => x.Worker)
                    .Include(x => x.MarkedBy)
                    .FirstOrDefaultAsync(x => x.Id == id);

            if (attendance == null)
            {
                return NotFound("Attendance record does not exist.");
            }

            if (!await CanAccessWorker(attendance.Worker.CampId))
            {
                return Forbid();
            }

            var result = new AttendanceResponseDTO
            {
                Id = attendance.Id,
                WorkerId = attendance.WorkerId,
                WorkerName = attendance.Worker.Name,
                AtDate = attendance.AtDate,
                Status = attendance.Status,
                MarkedBy = attendance.MarkedBy.Name
            };

            return Ok(result);
        }


       
        [Authorize(Roles = "Admin,Supervisor")]
        [HttpGet("by-worker/{workerId}")]
        public async Task<IActionResult> GetAttendanceByWorker( int workerId)
        {
            var worker =await _context.Workers.FirstOrDefaultAsync(x => x.Id == workerId);

            if (worker == null)
            {
                return NotFound( "Worker does not exist.");
            }

            if (!await CanAccessWorker(worker.CampId))
            {
                return Forbid();
            }

            var attendance = await _context.Attendances.Where(x => x.WorkerId == workerId)
                    .Include(x => x.Worker)
                    .Include(x => x.MarkedBy)
                    .OrderByDescending(x => x.AtDate)
                    .Select(x =>new AttendanceResponseDTO
                        {
                            Id = x.Id,
                            WorkerId = x.WorkerId,
                            WorkerName = x.Worker.Name,
                            AtDate = x.AtDate,
                            Status = x.Status,
                            MarkedBy = x.MarkedBy.Name
                        })
                    .ToListAsync();

            return Ok(attendance);
        }


      

        [Authorize(Roles = "Admin,Supervisor")]
        [HttpGet("by-date/{date}")]
        public async Task<IActionResult> GetAttendanceByDate( DateOnly date)
        {
            var query = _context.Attendances
                .Include(x => x.Worker)
                .Include(x => x.MarkedBy)
                .AsQueryable();

            if (IsSupervisor())
            {
                var supervisorCampId =await GetSupervisorCampId();

                if (supervisorCampId == null)
                {
                    return NotFound("You have not been assigned to any camp yet.");
                }

                query = query.Where(x => x.Worker.CampId ==supervisorCampId.Value);
            }

            var attendance = await query .Where(x => x.AtDate == date).Select(x =>
                        new
                        {
                            Id = x.Id,
                            WorkerId = x.WorkerId,
                            WorkerName = x.Worker.Name,
                            CampId = x.Worker.CampId,
                            CampName = x.Worker.Camp.Name,
                            AtDate = x.AtDate,
                            Status = x.Status,
                            MarkedBy = x.MarkedBy.Name
                        })
                    .ToListAsync();

            return Ok(attendance);
        }


       
        [Authorize(Roles = "Admin,Supervisor")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAttendance( int id,[FromBody] AttendanceUpdateDTO model)
        {
            if (model.Status is not ("Present" or "Absent" or "Leave"))
            {
                return BadRequest(
                    "Status must be Present, Absent or Leave.");
            }

            var attendance = await _context.Attendances.Include(x => x.Worker).FirstOrDefaultAsync( x => x.Id == id);

            if (attendance == null)
            {
                return NotFound("Attendance record does not exist");
            }

            if (!await CanAccessWorker( attendance.Worker.CampId))
            {
                return Forbid();
            }

            attendance.Status = model.Status;

            await _context.SaveChangesAsync();

            return Ok( "Attendance updated successfully");
        }


       

        [Authorize(
            Roles = "Worker,Admin,Supervisor,Driver")]
        [HttpGet("my-attendance")]
        public async Task<IActionResult> GetMyAttendance()
        {
            var userIdClaim =User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim,out int userId))
            {
                return Unauthorized("User ID not found in token.");
            }

            var role =User.FindFirst( ClaimTypes.Role)?.Value;


           

            if (string.Equals(role, "Driver",StringComparison.OrdinalIgnoreCase))
            {
                var driverAttendance =await _context.DriverAttendances.Where(x => x.DriverId == userId).OrderByDescending(x => x.AtDate)
                        .Select(x =>new AttendanceResponseDTO
                            {
                                Id = x.Id,
                                WorkerId = 0,
                                WorkerName =x.Driver.Name,
                                AtDate = x.AtDate,
                                Status = x.Status,
                                MarkedBy =x.MarkedBy.Name
                            })
                        .ToListAsync();

                return Ok(driverAttendance);
            }


          

            var worker = await _context.Workers.FirstOrDefaultAsync(x => x.UserId == userId);

            if (worker == null)
            {
                return NotFound( "Worker profile not found for this user.");
            }

            var attendance = await _context.Attendances.Where(x =>x.WorkerId == worker.Id).OrderByDescending( x => x.AtDate).Select(x =>
                        new AttendanceResponseDTO
                        {
                            Id = x.Id,
                            WorkerId = x.WorkerId,
                            WorkerName = x.Worker.Name,
                            AtDate = x.AtDate,
                            Status = x.Status,
                            MarkedBy =x.MarkedBy.Name
                        })
                    .ToListAsync();

            return Ok(attendance);
        }


       

        [Authorize(Roles = "Admin,Supervisor")]
        [HttpGet("summary/{workerId}")]
        public async Task<IActionResult> GetAttendanceSummary(int workerId)
        {
            var worker =await _context.Workers.FirstOrDefaultAsync( x => x.Id == workerId);

            if (worker == null)
            {
                return NotFound("Worker does not exist");
            }

            if (!await CanAccessWorker(worker.CampId))
            {
                return Forbid();
            }

            var summary =new AttendanceSummaryDTO
                {
                WorkerId = workerId,
                TotalPresent =await _context.Attendances.CountAsync(x => x.WorkerId == workerId && x.Status == "Present"),
                TotalAbsent =await _context.Attendances.CountAsync(x => x.WorkerId == workerId && x.Status == "Absent"),
                TotalLeave =  await _context.Attendances.CountAsync(x =>x.WorkerId == workerId && x.Status == "Leave")
                };

            return Ok(summary);
        }


      

        private bool IsSupervisor()
        {
            var role =User.FindFirst(ClaimTypes.Role)?.Value;

            return string.Equals( role, "Supervisor",StringComparison.OrdinalIgnoreCase);
        }


       

        private async Task<int?> GetSupervisorCampId()
        {
            var userIdClaim =User.FindFirst( ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse( userIdClaim, out var userId))
            {
                return null;
            }

            var supervisor =await _context.Supervisors.AsNoTracking().FirstOrDefaultAsync( x => x.UserId == userId);

            return supervisor?.CampId;
        }



        private async Task<bool> CanAccessWorker(int workerCampId)
        {
            var role =User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.Equals(role, "Admin",StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (string.Equals( role, "Supervisor",StringComparison.OrdinalIgnoreCase))
            {
                var supervisorCampId = await GetSupervisorCampId();

                return supervisorCampId.HasValue &&supervisorCampId.Value == workerCampId;
            }

            return false;
        }
    }
}