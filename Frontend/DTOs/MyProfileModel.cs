namespace Frontend.DTOs
{
    public class MyProfileModel
    {
        public int UserId { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Role { get; set; } = "";
        public WorkerResponseModel? Worker { get; set; }
        public SupervisorProfileModel? Supervisor { get; set; }
        public MyBusProfileModel? Bus { get; set; }
        public List<AttendanceResponseModel> Attendance { get; set; } = new();
        public List<DriverAttendanceProfileModel> DriverAttendance { get; set; } = new();
        public List<LeaveResponseModel> Leaves { get; set; } = new();
    }
    public class SupervisorProfileModel
    {
        public int Id { get; set; }
        public int CampId { get; set; }
        public string CampName { get; set; } = "";
        public string CampLocation { get; set; } = "";
        public int TotalRooms { get; set; }
        public int TotalBeds { get; set; }
        public int OccupiedBeds { get; set; }
    }

    public class MyBusProfileModel
    {
        public int Id { get; set; }
        public int BusNo { get; set; }
        public int Capacity { get; set; }
        public int TotalWorkersAssigned { get; set; }
        public List<RouteStopResponseModel> RouteStops { get; set; } = new();
    }
    public class DriverAttendanceProfileModel
    {
        public int Id { get; set; }
        public DateOnly AtDate { get; set; }
        public string Status { get; set; } = "";
        public string MarkedBy { get; set; } = "";
    }
}
