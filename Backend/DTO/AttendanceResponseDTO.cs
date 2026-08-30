namespace Backend.DTO
{
    public class AttendanceResponseDTO
    {
        public int Id { get; set; }

        public int WorkerId { get; set; }

        public string WorkerName { get; set; }

        public DateOnly AtDate { get; set; }

        public string Status { get; set; }

        public string MarkedBy { get; set; }
    }
}