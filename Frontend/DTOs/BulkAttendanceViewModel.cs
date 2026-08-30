namespace Frontend.DTOs
{
    public class BulkAttendanceViewModel
    {
        public int CampId { get; set; }
        public DateOnly AtDate { get; set; }
        public List<WorkerAttendanceItem> Attendances { get; set; }= new List<WorkerAttendanceItem>();
    }


    public class WorkerAttendanceItem
    {
        public int WorkerId { get; set; }
        public string Status { get; set; } = "Present";
    }
}