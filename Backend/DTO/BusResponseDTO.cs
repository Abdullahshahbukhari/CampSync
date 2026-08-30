namespace Backend.DTO
{
    public class BusResponseDTO
    {
        public int Id { get; set; }
        public int BusNo { get; set; }
        public string DriverName { get; set; }
        public int TotalWorkersAssigned { get; set; }
        public int Capacity { get; set; }
        public string Route { get; set; }
        public int DriverId { get; set; }
        public List<RouteStopResponseDTO> RouteStops { get; set; } = new();
    }
}
