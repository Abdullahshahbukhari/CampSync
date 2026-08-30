using Backend.Data;
using Backend.DTO;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using System.Security.Claims;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RouteStopController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RouteStopController(ApplicationDbContext context)
        {
            _context = context;
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateRout(
      [FromBody] RouteStopCreateDTO model)
        {
            var existingBus = await _context.Buses.FirstOrDefaultAsync(x => x.Id == model.BusId);
            if (existingBus == null)
            {
                return NotFound("Bus does not exist");
            }

            if (string.IsNullOrWhiteSpace(model.StopName))
            {
                return BadRequest("Stop name is required");
            }

            if (model.SeqOrder < 1)
            {
                return BadRequest("Sequence order must be greater than 0");
            }


            var existingStops = await _context.Rout_Stops.Where(x =>x.BusId == model.BusId &&x.SeqOerder >= model.SeqOrder).OrderByDescending(x => x.SeqOerder).ToListAsync();

            foreach (var stop in existingStops)
            {
                stop.SeqOerder += 1;
            }
            var routeStop = new Rout_Stop
            {
                BusId = model.BusId,
                Name = model.StopName,
                SeqOerder = model.SeqOrder
            };
            _context.Rout_Stops.Add(routeStop);
            await _context.SaveChangesAsync();
            return Ok("New Route Stop is created successfully");
        }


        [Authorize(Roles = "Admin")]
        [HttpPost("bulk-create")]
        public async Task<IActionResult> CreateMultipleRouteStops([FromBody] RouteStopBulkCreateDTO model)
        {
            if (model == null || model.Stops == null || !model.Stops.Any())
            {
                return BadRequest("Please add at least one route stop");
            }
            var existingBus = await _context.Buses.FirstOrDefaultAsync(x => x.Id == model.BusId);
            if (existingBus == null)
            {
                return NotFound("Bus does not exist");
            }
            if (model.Stops.Any(x =>
                string.IsNullOrWhiteSpace(x.StopName)))
            {
                return BadRequest("All stop names are required");
            }

            if (model.Stops.Any(x => x.SeqOrder < 1))
            {
                return BadRequest( "Sequence order must be greater than 0");
            }

            var duplicateSequence = model.Stops.GroupBy(x => x.SeqOrder).Any(x => x.Count() > 1);
            if (duplicateSequence)
            {
                return BadRequest("Two route stops cannot have the same sequence order");
            }
            using var transaction =await _context.Database.BeginTransactionAsync();

            try
            {
                var orderedStops = model.Stops.OrderBy(x => x.SeqOrder).ToList();
                foreach (var item in orderedStops)
                {
                    var stopsToShift = await _context.Rout_Stops .Where(x => x.BusId == model.BusId && x.SeqOerder >= item.SeqOrder).OrderByDescending(x => x.SeqOerder) .ToListAsync();
                    foreach (var existingStop in stopsToShift)
                    {
                        existingStop.SeqOerder += 1;
                    }
                    var routeStop = new Rout_Stop
                    {
                        BusId = model.BusId,
                        Name = item.StopName,
                        SeqOerder = item.SeqOrder
                    };
                    await _context.Rout_Stops.AddAsync(routeStop);
                    await _context.SaveChangesAsync();
                }
                await transaction.CommitAsync();
                return Ok(  "Route stops created successfully" );
            }
            catch
            {
                await transaction.RollbackAsync();
                return StatusCode(500,"An error occurred while creating route stops");
            }
        }



        [Authorize(Roles = "Admin,Supervisor,Driver")]
        [HttpGet("by-bus/{busId}")]
        public async Task<IActionResult> GetRouteStopsByBus(int busId)
        {
            var bus = await _context.Buses.FirstOrDefaultAsync(x => x.Id == busId);
            if (bus == null)
            {
                return NotFound("Bus does not exist");
            }

            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role == "Driver")
            {
                var userIdClaim =User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                int currentUserId = int.Parse(userIdClaim);
                if (bus.DriverId != currentUserId)
                {
                    return Forbid();
                }
            }

            var stops = await _context.Rout_Stops.Where(x => x.BusId == busId).OrderBy(x => x.SeqOerder).Select(x => new RouteStopResponseDTO
                {
                    Id = x.Id,
                    StopName = x.Name,
                    SeqOrder = x.SeqOerder,
                    BusId = x.BusId,
                    BusNo = x.Bus.BusNo
                })
                .ToListAsync();

            return Ok(stops);

        }

        [Authorize(Roles = "Admin,Supervisor")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRouteStopById(int id)
        {
            var stop = await _context.Rout_Stops.Where(x => x.Id == id)
                .Select(x => new RouteStopResponseDTO
                {
                    Id = x.Id,
                    StopName = x.Name,
                    SeqOrder = x.SeqOerder,
                    BusNo = x.Bus.BusNo
                })
                .FirstOrDefaultAsync();

            if (stop == null)
            {
                return NotFound("Route stop does not exist");
            }

            return Ok(stop);
        }


        [Authorize(Roles = "Admin,Supervisor")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStop(int id,[FromBody] RouteStopUpdateDTO model)
        {
            var stop = await _context.Rout_Stops.FirstOrDefaultAsync(x => x.Id == id);

            if (stop == null)
            {
                return NotFound("No stop exists");
            }
            if (string.IsNullOrWhiteSpace(model.StopName))
            {
                return BadRequest("Stop name is required");
            }
            if (model.SeqOrder < 1)
            {
                return BadRequest("Sequence order must be greater than 0");
            }
            var oldSequence = stop.SeqOerder;
            var newSequence = model.SeqOrder;
            var busId = stop.BusId;
            var totalStops = await _context.Rout_Stops.CountAsync(x => x.BusId == busId);
            if (newSequence > totalStops)
            {
                newSequence = totalStops;
            }
            using var transaction =await _context.Database.BeginTransactionAsync();
            try
            {
                if (newSequence < oldSequence)
                {
                    var stopsToShift = await _context.Rout_Stops.Where(x =>x.BusId == busId &&x.Id != id &&x.SeqOerder >= newSequence &&x.SeqOerder < oldSequence).ToListAsync();
                foreach (var item in stopsToShift)
                    {
                        item.SeqOerder += 1;
                    }
                }
                else if (newSequence > oldSequence)
                {
                    var stopsToShift = await _context.Rout_Stops .Where(x => x.BusId == busId &&x.Id != id &&x.SeqOerder > oldSequence &&x.SeqOerder <= newSequence).ToListAsync();
                    foreach (var item in stopsToShift)
                    {
                        item.SeqOerder -= 1;
                    }
                }
                stop.Name = model.StopName;
                stop.SeqOerder = newSequence;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok("Route stop updated successfully");
            }
            catch
            {
                await transaction.RollbackAsync();
                return StatusCode(500,"An error occurred while updating the route stop");
            }
        }




        [Authorize(Roles = "Admin")]
        [HttpDelete("{Id}")]
        public async Task<IActionResult> Deletestop(int id)
        {
            var stop = await _context.Rout_Stops.FirstOrDefaultAsync(x => x.Id == id);
            if (stop == null)
            {
                return NotFound("No stop exist");
            }

            _context.Rout_Stops.Remove(stop);
            await _context.SaveChangesAsync();
            return Ok("Stop is Deleted");
        }


        [Authorize(Roles = "Admin")]
        [HttpPut("reorder/{busId}")]
        public async Task<IActionResult> ReorderRouteStops(int busId, [FromBody] List<RouteStopReorderDTO> model)
        {
            var bus = await _context.Buses.FirstOrDefaultAsync(x => x.Id == busId);
            if (bus == null)
            {
                return NotFound("Bus does not exist");
            }
            var inputstopids = model.Select(x => x.StopId).ToList();
            var existingstop = await _context.Rout_Stops.Where(x => x.BusId == busId && inputstopids.Contains(x.Id)).ToListAsync();
            if (existingstop.Count != model.Count)
            {
                return BadRequest("One or more stops do not belong to this bus, or do not exist");
            }
            foreach (var stopUpdate in model)
            {
                var stop = existingstop.First(x => x.Id == stopUpdate.StopId);
                stop.SeqOerder = stopUpdate.NewSeqOrder;
            }
            await _context.SaveChangesAsync();

            return Ok("Route stops reordered successfully");
        }
    }

}
