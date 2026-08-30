namespace Frontend.DTOs
{
    public class BusResponseModel
    {
        public int Id { get; set; }
        public int BusNo { get; set; }
        public string? DriverName { get; set; }
        public int TotalWorkersAssigned { get; set; }
        public int Capacity { get; set; }
        public string? Route { get; set; }
        public int DriverId { get; set; }

        public List<RouteStopResponseModel> RouteStops { get; set; } = new();
    }
}