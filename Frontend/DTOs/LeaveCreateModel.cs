namespace Frontend.DTOs
{
    public class LeaveCreateModel
    {
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
