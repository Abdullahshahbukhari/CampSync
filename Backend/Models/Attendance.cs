namespace Backend.Models
{
    public class Attendance
    {
        public int Id { get; set; }
        public DateOnly AtDate { get; set; }
        public string Status { get; set; }
        public int WorkerId { get; set; }
        public Worker Worker { get; set; }
        public int UserId { get; set; }
        public User MarkedBy { get; set; }

    }
}
