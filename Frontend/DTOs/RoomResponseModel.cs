namespace Frontend.DTOs
{
    public class RoomResponseModel
    {
        public int Id { get; set; }
        public int RoomNo { get; set; }
        public int CampId { get; set; }
        public string CampName { get; set; } = string.Empty;
        public int TotalBeds { get; set; }
        public int OccupiedBeds { get; set; }
        public int AvailableBeds { get; set; }
    }
}
