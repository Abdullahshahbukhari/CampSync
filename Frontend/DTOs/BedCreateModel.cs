using System.ComponentModel.DataAnnotations;

namespace Frontend.DTOs
{
    public class BedCreateModel
    {
        [Required(ErrorMessage = "Bed number is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Bed number must be greater than 0")]
        public int BedNo { get; set; }

        [Required(ErrorMessage = "Please select a room")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid room")]
        public int RoomId { get; set; }
    }
}