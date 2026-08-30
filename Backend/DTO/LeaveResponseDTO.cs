namespace Backend.DTO
{
    public class LeaveResponseDTO
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }
        public string? CampName { get; set; }
        public string? ApprovedByName { get; set; }
        public string? RejectionReason { get; set; }
    }
}
