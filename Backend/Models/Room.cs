namespace Backend.Models
{
    public class Room
    {
        public int Id { get; set; }
        public int RoomNo { get; set; }

        public int CampId { get; set; }
        public Camp Camp { get; set; }
        public ICollection<Bed> Beds { get; set; }
    }
}
