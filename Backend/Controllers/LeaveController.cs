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
    public class LeaveController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LeaveController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool TryGetCurrentUser(out int userId, out string role)
        {
            userId = 0;
            role = User.FindFirstValue(ClaimTypes.Role) ?? "";

            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claim, out userId);
        }

     
        [Authorize(Roles = "Admin,Supervisor,Worker,Driver")]
        [HttpPost]
        public async Task<IActionResult> NewLeave([FromBody] LeaveCreateDTO model)
        {
            if (!TryGetCurrentUser(out var userId, out _))
                return Unauthorized("User ID not found in token.");

            if (model.FromDate < DateOnly.FromDateTime(DateTime.Today))
                return BadRequest("Start date cannot be in the past.");

            if (model.ToDate < model.FromDate)
                return BadRequest("End date must be greater than or equal to start date.");

            var userExists = await _context.Users.AnyAsync(x => x.Id == userId);
            if (!userExists)
                return Unauthorized("Logged-in user does not exist.");

            var leave = new Leave
            {
                UserId = userId,
                FromeDate = model.FromDate,
                ToDate = model.ToDate,
                Reason = model.Reason,
                Status = "Pending"
            };

            _context.Leaves.Add(leave);
            await _context.SaveChangesAsync();

            return Ok("Leave request submitted successfully.");
        }

        [Authorize(Roles = "Admin,Supervisor,Worker,Driver")]
        [HttpGet("my-leaves")]
        public async Task<IActionResult> GetMyLeaves()
        {
            if (!TryGetCurrentUser(out var userId, out _))
                return Unauthorized("User ID not found in token.");

            var leaves = await _context.Leaves.Where(x => x.UserId == userId).OrderByDescending(x => x.FromeDate)
                .Select(x => new LeaveResponseDTO
                {
                    Id = x.Id,
                    UserName = x.User.Name,
                    Role = x.User.Role,
                    FromDate = x.FromeDate,
                    ToDate = x.ToDate,
                    Reason = x.Reason,
                    Status = x.Status,
                    CampName = _context.Workers.Where(w => w.UserId == x.UserId).Select(w => w.Camp.Name).FirstOrDefault(),
                    ApprovedByName = x.ApprovedByUser != null ? x.ApprovedByUser.Name : null,
                    RejectionReason = x.RejectionReason
                })
                .ToListAsync();

            return Ok(leaves);
        }

        
        [Authorize(Roles = "Admin,Supervisor")]
        [HttpGet("Pending")]
        public async Task<IActionResult> PendingLeaves()
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            var query = _context.Leaves.Where(x => x.Status == "Pending");
            if (role == "Supervisor")
            {
                query = query.Where(x => _context.Workers.Any(w => w.UserId == x.UserId && w.User.Role == "Worker"));
            }

            var leaves = await query .OrderByDescending(x => x.FromeDate).Select(x => new LeaveResponseDTO
                {
                    Id = x.Id,
                    UserName = x.User.Name,
                    Role = x.User.Role,
                    FromDate = x.FromeDate,
                    ToDate = x.ToDate,
                    Reason = x.Reason,
                    Status = x.Status,
                    CampName = _context.Workers
                        .Where(w => w.UserId == x.UserId)
                        .Select(w => w.Camp.Name)
                        .FirstOrDefault(),
                    ApprovedByName = x.ApprovedByUser != null ? x.ApprovedByUser.Name : null,
                    RejectionReason = x.RejectionReason
                })
                .ToListAsync();

            return Ok(leaves);
        }

        [Authorize(Roles = "Admin,Supervisor")]
        [HttpPut("approve/{id}")]
        public async Task<IActionResult> ApproveLeave(int id)
        {
            var leave = await _context.Leaves.Include(x => x.User) .FirstOrDefaultAsync(x => x.Id == id);

            if (leave == null)
                return NotFound("Leave request does not exist.");

            if (leave.Status != "Pending")
                return BadRequest("Leave request is already processed.");

            if (!TryGetCurrentUser(out var approverId, out var role))
                return Unauthorized("User ID not found in token.");

            if (role == "Supervisor")
            {
                var isWorker = await _context.Workers.AnyAsync(w => w.UserId == leave.UserId && w.User.Role == "Worker");

                if (!isWorker)
                    return Forbid();

                if (leave.UserId == approverId)
                    return BadRequest("You cannot approve your own leave.");
            }

            leave.Status = "Approved";
            leave.ApprovedBy = approverId;
            leave.RejectionReason = null;

            await _context.SaveChangesAsync();
            return Ok("Leave approved successfully.");
        }

      
        [Authorize(Roles = "Admin,Supervisor")]
        [HttpPut("{id}/reject")]
        public async Task<IActionResult> RejectLeave(int id, [FromBody] LeaveDecisionDTO model)
        {
            var leave = await _context.Leaves.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == id);

            if (leave == null)
                return NotFound("Leave request does not exist.");

            if (leave.Status != "Pending")
                return BadRequest("Leave request is already processed.");

            if (!TryGetCurrentUser(out var approverId, out var role))
                return Unauthorized("User ID not found in token.");

            if (role == "Supervisor")
            {
                var isWorker = await _context.Workers.AnyAsync(w => w.UserId == leave.UserId && w.User.Role == "Worker");

                if (!isWorker)
                    return Forbid();

                if (leave.UserId == approverId)
                    return BadRequest("You cannot reject your own leave.");
            }

            leave.Status = "Rejected";
            leave.ApprovedBy = approverId;
            leave.RejectionReason = string.IsNullOrWhiteSpace(model?.RejectionReason)? null: model.RejectionReason.Trim();
            await _context.SaveChangesAsync();
            return Ok("Leave rejected successfully.");
        }

        [Authorize(Roles = "Admin,Supervisor")]
        [HttpGet("by-worker/{workerId}")]
        public async Task<IActionResult> GetLeavesByWorker(int workerId)
        {
            var worker = await _context.Workers.Include(x => x.User).Include(x => x.Camp).FirstOrDefaultAsync(x => x.Id == workerId);
            if (worker == null)
                return NotFound("Worker does not exist.");

            var leaves = await _context.Leaves.Where(x => x.UserId == worker.UserId).OrderByDescending(x => x.FromeDate).Select(x => new LeaveResponseDTO
                {
                    Id = x.Id,
                    UserName = x.User.Name,
                    Role = x.User.Role,
                    FromDate = x.FromeDate,
                    ToDate = x.ToDate,
                    Reason = x.Reason,
                    Status = x.Status,
                    CampName = worker.Camp != null? worker.Camp.Name: null,
                    ApprovedByName = x.ApprovedByUser != null? x.ApprovedByUser.Name: null,
                    RejectionReason = x.RejectionReason
                })
                .ToListAsync();

            return Ok(leaves);
        }

        [Authorize(Roles = "Admin,Supervisor,Worker,Driver")]
        [HttpGet("summary/{workerId?}")]
        public async Task<IActionResult> GetLeaveSummary(int? workerId)
        {
            if (!TryGetCurrentUser(out var userId, out var role))
                return Unauthorized("User ID not found in token.");

            int targetUserId;
            int? targetWorkerId = null;

            if (!workerId.HasValue)
            {
                targetUserId = userId;
            }
            else
            {
                if (role != "Admin" && role != "Supervisor")
                    return Forbid();

                var worker = await _context.Workers.FirstOrDefaultAsync(x => x.Id == workerId.Value);

                if (worker == null || !worker.UserId.HasValue)
                    return NotFound("Worker does not exist or has no user account.");

                targetWorkerId = worker.Id;
                targetUserId = worker.UserId.Value;
            }

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == targetUserId);
            if (user == null)
                return NotFound("User does not exist.");

            var summary = await _context.Leaves.Where(x => x.UserId == targetUserId).GroupBy(x => x.Status)
                .Select(x => new LeaveSummaryItemDTO
                {
                    Status = x.Key,
                    TotalLeaves = x.Count()
                })
                .ToListAsync();

            return Ok(new LeaveSummaryResponseDTO
            {
                UserId = targetUserId,
                WorkerId = targetWorkerId,
                UserName = user.Name,
                Role = user.Role,
                Summary = summary
            });
        }
    }
}
