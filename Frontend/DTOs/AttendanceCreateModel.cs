namespace Frontend.DTOs
{
    public class AttendanceCreateModel
    {
        public int WorkerId { get; set; }

        public DateOnly AtDate { get; set; }

        public string Status { get; set; }
    }
}