namespace Backend.DTO
{
    public class MyProfileResponseDTO
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public WorkerResponseDTO? Worker { get; set; }
        public SupervisorProfileDTO? Supervisor { get; set; }
        public MyBusProfileDTO? Bus { get; set; }
        public List<AttendanceResponseDTO> Attendance { get; set; } = new();
        public List<DriverAttendanceProfileDTO> DriverAttendance { get; set; } = new();
        public List<LeaveResponseDTO> Leaves { get; set; } = new();
    }

    public class SupervisorProfileDTO
    {
        public int Id { get; set; }
        public int CampId { get; set; }
        public string CampName { get; set; } = string.Empty;
        public string CampLocation { get; set; } = string.Empty;
        public int TotalRooms { get; set; }
        public int TotalBeds { get; set; }
        public int OccupiedBeds { get; set; }
    }

    public class MyBusProfileDTO
    {
        public int Id { get; set; }
        public int BusNo { get; set; }
        public int Capacity { get; set; }
        public int TotalWorkersAssigned { get; set; }
        public List<RouteStopResponseDTO> RouteStops { get; set; } = new();
    }

    public class DriverAttendanceProfileDTO
    {
        public int Id { get; set; }
        public DateOnly AtDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string MarkedBy { get; set; } = string.Empty;
    }
}