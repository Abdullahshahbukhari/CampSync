namespace Backend.DTO
{
    public class BedOccupancyDTO
    {
        public string CampName { get; set; }
        public int TotalBeds { get; set; }
        public int OccupiedBes { get; set; }
        public decimal OccupancyRate { get; set; }

    }
}
