using System.ComponentModel.DataAnnotations;

namespace Frontend.DTOs
{
    public class BusCreateModel
    {
        [Required(ErrorMessage = "Bus number is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Bus number must be greater than 0")]
        public int BusNo { get; set; }

        [Required(ErrorMessage = "Please select a driver")]
        public int DriverId { get; set; }

        [Required(ErrorMessage = "Capacity is required")]
        [Range(1, 1000, ErrorMessage = "Capacity must be between 1 and 1000")]
        public int Capacity { get; set; }
    }
}