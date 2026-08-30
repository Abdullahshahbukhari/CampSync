using Backend.Data;
using Backend.DTO;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RoomController(ApplicationDbContext context)
        {
            _context = context;
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateRoom([FromBody] RoomCreateDTO model)
        {
            var camp = await _context.Camps.FirstOrDefaultAsync(x => x.Id == model.CampId);
            if (camp == null)
            {
                return BadRequest("Camp does not exist");
            }
            var existingRoom = await _context.Rooms.FirstOrDefaultAsync(x =>x.CampId == model.CampId &&x.RoomNo == model.RoomNo);

            if (existingRoom != null)
            {
                return BadRequest("This room number already exists in this camp.");
            }

            var room = new Room
            {
                RoomNo = model.RoomNo,
                CampId = model.CampId
            };

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();
            return Ok("Room is created");
        }


        [Authorize(Roles = "Admin,Supervisor")]
        [HttpGet("by-camp/{campId}")]
        public async Task<IActionResult> GetRoomsByCamp(int CampId)
        {

            var room = await _context.Rooms.Where(x => x.CampId == CampId).Include(x => x.Camp).Include(x => x.Beds).Select(
                x => new RoomResponseDTO
                {
                    Id = x.Id,
                    RoomNo = x.RoomNo,
                    CampName = x.Camp.Name,
                    CampId = x.CampId,
                    TotalBeds = x.Beds.Count(),
                    AvailableBeds = x.Beds.Count(b => b.Isoccupied == false),
                    OccupiedBeds = x.Beds.Count(b => b.Isoccupied == true),
                }
                ).ToListAsync();
            return Ok(room);

        }

        [Authorize(Roles = "Admin,Supervisor")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoomDetail(int id)
        {
            var room = await _context.Rooms.Where(x => x.Id == id).Include(x => x.Camp).Include(x => x.Beds)
                .Select(x => new RoomResponseDTO
                {
                    Id = x.Id,
                    RoomNo = x.RoomNo,
                    CampName = x.Camp.Name,
                    CampId = x.CampId,
                    TotalBeds = x.Beds.Count(),
                    OccupiedBeds = x.Beds.Count(b => b.Isoccupied == true),
                    AvailableBeds = x.Beds.Count(b => b.Isoccupied == false)
                })
                .FirstOrDefaultAsync();

            if (room == null)
            {
                return NotFound("Room does not exist");
            }

            return Ok(room);
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> RoomUpdate(int id, [FromBody] RoomUpdateDTO model)
        {
            var room = await _context.Rooms.FirstOrDefaultAsync(x => x.Id == id);
            if (room == null)
            {
                return NotFound("Room does not exist");
            }
            var campExists = await _context.Camps.AnyAsync(x => x.Id == model.CampId);
            if (!campExists)
            {
                return NotFound("No camp with this Id");
            }

            var duplicateRoom = await _context.Rooms .AnyAsync(x => x.CampId == model.CampId && x.RoomNo == model.RoomNo &&x.Id != id);
            if (duplicateRoom)
            {
                return BadRequest("This room number already exists in this camp.");
            }

            room.CampId = model.CampId;
            room.RoomNo = model.RoomNo;

            await _context.SaveChangesAsync();
            return Ok("Room Updated");
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            var room = await _context.Rooms.FirstOrDefaultAsync(x => x.Id == id);
            if (room == null)
            {
                return NotFound("Room does not exist");
            }
            bool hasOccupiedBed = await _context.Beds.AnyAsync(x => x.RoomId == id && x.Isoccupied == true);
            if (hasOccupiedBed)
            {
                return BadRequest("A bed in this room is occupied. Room cannot be deleted");
            }


            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
            return Ok("Room deleted successfully");
        }


    }
}
