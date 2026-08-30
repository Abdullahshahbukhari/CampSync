namespace Backend.DTO
{
    public class LeaveCreateDTO
    {
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }
        public string Reason { get; set; }
    }
}
