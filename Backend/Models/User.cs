namespace Backend.Models
{
    public class User
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Passwordhash { get; set; }

        public string Role { get; set; }

        public ICollection<Bus> Bus { get; set; }

        public ICollection<Attendance> Attendances { get; set; }

        public ICollection<DriverAttendance> DriverAttendances { get; set; }

        public ICollection<DriverAttendance> MarkedDriverAttendances { get; set; }

        public ICollection<Leave> Leaves { get; set; }

        public ICollection<Leave> ApprovedBy { get; set; }

        public Supervisor? Supervisor { get; set; }
    }
}
