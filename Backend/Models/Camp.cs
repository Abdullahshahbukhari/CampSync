namespace Backend.Models
{
    public class Camp
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public ICollection<Room> Rooms { get; set; }
        public ICollection<Worker> Workers { get; set; }
        public ICollection<Supervisor> Supervisors { get; set; }
    }
}
