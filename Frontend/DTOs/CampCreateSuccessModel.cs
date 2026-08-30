namespace Frontend.DTOs
{
    public class CampCreateSuccessModel
    {
        public string Message { get; set; } = string.Empty;
        public int CampId { get; set; }
        public string Supervisor { get; set; } = string.Empty;
    }
}
