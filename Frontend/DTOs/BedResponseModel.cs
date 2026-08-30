namespace Frontend.DTOs
{
    public class BedResponseModel
    {
        public int Id { get; set; }
        public int BedNo { get; set; }
        public bool IsOccupied { get; set; }
        public int RoomNo { get; set; }
    }
}