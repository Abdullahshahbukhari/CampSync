using System.ComponentModel.DataAnnotations;

namespace Frontend.DTOs
{
    public class CampUpdateModel
    {
        [Required(ErrorMessage = "Camp name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Location is required")]
        public string Location { get; set; }
    }
}