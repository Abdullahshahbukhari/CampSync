namespace Backend.Models
{
    public class Supervisor
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public User User { get; set; } = null!;

        public int CampId { get; set; }

        public Camp Camp { get; set; } = null!;
    }
}