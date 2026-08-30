namespace Backend.DTO
{
    public class BedResponseDTO
    {
        public int Id { get; set; }
        public int BedNo { get; set; }
        public bool IsOccupied { get; set; }
        public int RoomNo { get; set; }
        public string CampName { get; set; }
    }
}
