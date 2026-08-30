namespace Backend.DTO
{
    public class RoomResponseDTO
    {
        public int Id { get; set; }
        public int RoomNo { get; set; }
        public string CampName { get; set; }
        public int TotalBeds { get; set; }
        public int OccupiedBeds { get; set; }
        public int AvailableBeds { get; set; }
        public int CampId { get; set; }
        public int BusId { get; set; }
        public int BedId { get; set; }
    }
}
