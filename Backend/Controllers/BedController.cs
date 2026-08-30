using Backend.Data;
using Backend.DTO;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BedController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BedController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateBed([FromBody] BedCreateDTO model)
        {
            var room = await _context.Rooms.FirstOrDefaultAsync(x => x.Id == model.RoomId);
            if (room == null)
            {
                return NotFound("Room does not exist");
            }
            var duplicateBed = await _context.Beds .AnyAsync(x => x.RoomId == model.RoomId &&x.BedNo == model.BedNo);

            if (duplicateBed)
            {
                return BadRequest( $"Bed number {model.BedNo} already exists in this room." );
            }

            var bed = new Bed
            {
                BedNo = model.BedNo,
                RoomId = model.RoomId,
                Isoccupied = false
            };

            await _context.Beds.AddAsync(bed);
            await _context.SaveChangesAsync();
            return Ok("Bed created successfully");
        }

        [Authorize(Roles = "Admin,Supervisor")]
        [HttpGet("by-room/{roomId}")]
        public async Task<IActionResult> GetBedsByRoom( int roomId, int? currentBedId = null)
        {
            var beds = await _context.Beds.Where(x => x.RoomId == roomId).Select(x => new BedResponseDTO
                {
                    Id = x.Id,
                    BedNo = x.BedNo,
                    IsOccupied = x.Isoccupied,
                    RoomNo = x.Room.RoomNo
                })
                .ToListAsync();

            return Ok(beds);
        }
        [Authorize(Roles = "Admin,Supervisor")]
        [HttpGet("by-camp/{campId}")]
        public async Task<IActionResult> GetAvailableBedsByCamp(int campId)
        {
            var availableBeds = await _context.Beds.Where(x => x.Room.CampId == campId && x.Isoccupied == false)
                .Select(x => new BedResponseDTO
                {
                    Id = x.Id,
                    BedNo = x.BedNo,
                    IsOccupied = x.Isoccupied,
                    RoomNo = x.Room.RoomNo
                })
                .ToListAsync();

            return Ok(availableBeds);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBed(int id, [FromBody] BedUpdateDTO model)
        {
            var bed = await _context.Beds.FirstOrDefaultAsync(x => x.Id == id);
            if (bed == null)
            {
                return NotFound("Bed does not exist");
            }

            if (model.RoomId != bed.RoomId)
            {
                var newRoom = await _context.Rooms.FirstOrDefaultAsync(x => x.Id == model.RoomId);
                if (newRoom == null)
                {
                    return NotFound("No room with this Id");
                }
                var duplicateBed = await _context.Beds.AnyAsync(x =>x.Id != id && x.RoomId == model.RoomId && x.BedNo == model.BedNo);

                if (duplicateBed)
                {
                    return BadRequest(
                        $"Bed number {model.BedNo} already exists in this room."
                    );
                }
                bed.RoomId = model.RoomId;
            }

            bed.BedNo = model.BedNo;

            _context.Beds.Update(bed);
            await _context.SaveChangesAsync();
            return Ok("Bed updated successfully");
        }

        [Authorize(Roles = "Admin,Supervisor")]
        [HttpGet("available-by-room/{roomId}")]
        public async Task<IActionResult> GetAvailableBedsByRoom(int roomId)
        {
            var beds = await _context.Beds.Where(x => x.RoomId == roomId && x.Isoccupied == false )
                .Select(x => new BedResponseDTO
                {
                    Id = x.Id,
                    BedNo = x.BedNo,
                    IsOccupied = x.Isoccupied,
                    RoomNo = x.Room.RoomNo
                })
                .ToListAsync();

            return Ok(beds);
        }


        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBed(int id)
        {
            var bed = await _context.Beds.FirstOrDefaultAsync(x => x.Id == id);
            if (bed == null)
            {
                return NotFound("Bed does not exist");
            }

            if (bed.Isoccupied == true)
            {
                return BadRequest("Bed is currently occupied and cannot be deleted");
            }

            _context.Beds.Remove(bed);
            await _context.SaveChangesAsync();
            return Ok("Bed deleted successfully");
        }
    }
}