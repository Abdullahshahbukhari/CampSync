namespace Backend.DTO
{
    public class RouteStopBulkCreateDTO
    {
        public int BusId { get; set; }

        public List<RouteStopCreateDTO> Stops { get; set; }
    }
}