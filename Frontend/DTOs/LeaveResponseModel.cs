namespace Frontend.DTOs
{
    public class LeaveResponseModel
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? CampName { get; set; }
        public string? ApprovedByName { get; set; }
        public string? RejectionReason { get; set; }
    }
}
