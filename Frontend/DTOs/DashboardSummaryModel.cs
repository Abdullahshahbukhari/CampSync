namespace Frontend.DTOs
{
    public class DashboardSummaryModel
    {
        public int TotalWorker { get; set; }
        public int TotalBuses { get; set; }
        public int TotalCampuses { get; set; }
        public decimal TodayAttendancePercentage { get; set; }
    }
}
