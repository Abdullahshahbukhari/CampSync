namespace Backend.DTO
{
    public class DriverAttendanceCreateDTO
    {
        public int DriverId { get; set; }
        public DateOnly AtDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
