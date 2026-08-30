namespace Backend.DTO
{
    public class DashboardSummaryDTO
    {
        public int TotalWorker { get; set; }
        public int TotalBuses { get; set; }
        public int TotalCampuses { get; set; }
        public decimal TodayAttendancePercentage { get; set; }
    }
}
