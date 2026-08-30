using System.ComponentModel.DataAnnotations;

namespace Frontend.DTOs
{
    public class CampCreateModel
    {
        [Required(ErrorMessage = "Camp name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Location is required")]
        public string Location { get; set; }

        [Required(ErrorMessage = "Please select a supervisor.")]
        public int SupervisorUserId { get; set; }


    }
}