namespace Backend.Models
{
    public class Bus
    {
        public int Id { get; set; }
        public int BusNo { get; set; }
        public int DriverId { get; set; }
        public User Driver { get; set; }
        public int Capacity { get; set; }
        public ICollection<Worker> Workers { get; set; }
        public ICollection<Rout_Stop> Route_Stop { get; set; }
    }
}
