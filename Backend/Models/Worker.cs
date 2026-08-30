namespace Backend.Models
{
    public class Worker
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string IqamaNo { get; set; }
        public string Nationality { get; set; }
        public string Trade { get; set; }
        public int CampId { get; set; }
        public Camp Camp { get; set; }
        public int BedId { get; set; }
        public Bed Bed { get; set; }
        public int BusId { get; set; }
        public Bus Bus { get; set; }
        public int? UserId { get; set; }
        public User User { get; set; }
        public ICollection<Attendance> Attendances { get; set; }
    }
}
