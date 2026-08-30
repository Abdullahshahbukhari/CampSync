namespace Backend.DTO
{
    public class LeaveSummaryResponseDTO
    {
        public int UserId { get; set; }
        public int? WorkerId { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
        public List<LeaveSummaryItemDTO> Summary { get; set; } = new();
    }
}
