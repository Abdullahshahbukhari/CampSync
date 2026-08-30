namespace Backend.DTO
{
    public class SupervisorResponseDTO
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string SupervisorName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public int CampId { get; set; }

        public string CampName { get; set; } = string.Empty;

        public string CampLocation { get; set; } = string.Empty;
    }
}