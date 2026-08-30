namespace Frontend.DTOs
{
    public class LeaveSummaryModel
    {
        public int UserId { get; set; }
        public int? WorkerId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public List<LeaveSummaryItemModel> Summary { get; set; } = new();
    }

    public class LeaveSummaryItemModel
    {
        public string Status { get; set; } = string.Empty;
        public int TotalLeaves { get; set; }
    }
}
