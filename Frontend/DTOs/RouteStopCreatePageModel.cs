namespace Frontend.DTOs
{
    public class RouteStopCreatePageModel
    {
        public int BusId { get; set; }
        public List<RouteStopCreateModel> Stops { get; set; }= new List<RouteStopCreateModel>();
    }
}