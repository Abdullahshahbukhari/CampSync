namespace Backend.Models
{
    public class DriverAttendance
    {
        public int Id { get; set; }
        public DateOnly AtDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int DriverId { get; set; }
        public User Driver { get; set; } = null!;
        public int MarkedByUserId { get; set; }
        public User MarkedBy { get; set; } = null!;
    }
}
