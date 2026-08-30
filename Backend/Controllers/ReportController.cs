using Backend.Data;
using Backend.DTO;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("dashboard-summary")]
        public async Task<IActionResult> DashboardSummary()
        {
            var totalWorkers = await _context.Workers.CountAsync();
            var totalCamps = await _context.Camps.CountAsync();
            var totalBuses = await _context.Buses.CountAsync();

            var today = DateOnly.FromDateTime(DateTime.Today);
            var totalAttendanceToday = await _context.Attendances.CountAsync(x => x.AtDate == today);
            var presentToday = await _context.Attendances.CountAsync(x => x.AtDate == today && x.Status == "Present");
            decimal todayAttendancePercentage = totalAttendanceToday == 0? 0: Math.Round((decimal)presentToday / totalAttendanceToday * 100, 2);
            var summary = new DashboardSummaryDTO
            {
                TotalWorker = totalWorkers,
                TotalCampuses = totalCamps,
                TotalBuses = totalBuses,
                TodayAttendancePercentage = todayAttendancePercentage
            };

            return Ok(summary);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("attendance-percentage")]
        public async Task<IActionResult> GetAttendancePercentage()
        {
            var grouped = await _context.Attendances.GroupBy(x => new { x.Worker.CampId, x.Worker.Camp.Name }).Select(g => new
            {
                CampName = g.Key.Name,
                TotalRecords = g.Count(),
                PresentRecords = g.Count(a => a.Status == "Present")
            }).ToListAsync();
            var result = grouped.Select(g => new AttendancePercentageDTO
            {
                CampName = g.CampName,
                Percentage = g.TotalRecords == 0
                    ? 0
                    : Math.Round((decimal)g.PresentRecords / g.TotalRecords * 100, 2)
            }).ToList();
            return Ok(result);
        }


        [Authorize(Roles = "Admin")]
        [HttpGet("Bed_Occupancy")]
        public async Task<IActionResult> Bed_Occupancy()
        {
            var Groupedby = await _context.Beds.GroupBy(x => new { x.Room.CampId, x.Room.Camp.Name }).Select(g => new
            {
                CampName = g.Key.Name,
                TotalRecords = g.Count(),
                PresentRecords = g.Count(a => a.Isoccupied == true)
            }).ToListAsync();

            var result = Groupedby.Select(g => new AttendancePercentageDTO
            {
                CampName = g.CampName,
                Percentage = g.TotalRecords == 0? 0: Math.Round((decimal)g.PresentRecords / g.TotalRecords * 100, 2)
            }).ToList();
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("bus-utilization")]
        public async Task<IActionResult> GetBusUtilization()
        {
            var buses = await _context.Buses.Select(x => new
                {
                    BusNo = x.BusNo,
                    Capacity = x.Capacity,
                    AssignedWorkers = x.Workers.Count()
                })
                .ToListAsync();

            var result = buses.Select(b => new BusUtilizationDTO
            {
                BusNo = b.BusNo,
                Capacity = b.Capacity,
                Assignedworker = b.AssignedWorkers,
                UtilizationRate = b.Capacity == 0? 0: Math.Round((decimal)b.AssignedWorkers / b.Capacity * 100, 2)
            }).ToList();

            return Ok(result);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("leave-trends")]
        public async Task<IActionResult> GetLeaveTrends()
        {
            var trends = await _context.Leaves.GroupBy(x => new { x.FromeDate.Year, x.FromeDate.Month }).Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalLeaves = g.Count()
                }).OrderBy(x => x.Year).ThenBy(x => x.Month).ToListAsync();

            var result = trends.Select(t => new LeaveTrendDTO
            {
                Month = $"{System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(t.Month)} {t.Year}",
                TotalLeaves = t.TotalLeaves
            }).ToList();

            return Ok(result);
        }
    }
}
