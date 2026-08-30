namespace Backend.DTO
{
    public class AttendanceCreateDTO
    {
        public int workerId { get; set; }

        public DateOnly AtDate { get; set; }

        public string Status { get; set; }
    }
}
