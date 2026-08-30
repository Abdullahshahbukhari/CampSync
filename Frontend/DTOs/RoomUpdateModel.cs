using System.ComponentModel.DataAnnotations;

namespace Frontend.DTOs
{
    public class RoomUpdateModel
    {
        [Required(ErrorMessage = "Room number is required")]
        public int RoomNo { get; set; }

        [Required(ErrorMessage = "Camp is required")]
        public int CampId { get; set; }
    }
}
