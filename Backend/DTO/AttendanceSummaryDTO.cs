namespace Backend.DTO
{
    public class AttendanceSummaryDTO
    {
        public int WorkerId { get; set; }
        public int TotalPresent { get; set; }
        public int TotalAbsent { get; set; }
        public int TotalLeave { get; set; }
    }
}
