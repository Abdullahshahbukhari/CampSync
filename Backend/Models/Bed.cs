namespace Backend.Models
{
    public class Bed
    {
        public int Id { get; set; }
        public int BedNo { get; set; }
        public bool Isoccupied { get; set; }
        public int RoomId { get; set; }
        public Room Room { get; set; }
        public Worker Worker { get; set; }
    }
}
