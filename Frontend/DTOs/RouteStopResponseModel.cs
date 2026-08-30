namespace Frontend.DTOs
{
    public class RouteStopResponseModel
    {
        public int Id { get; set; }
        public string StopName { get; set; }
        public int SeqOrder { get; set; }
        public int BusId { get; set; }
        public int BusNo { get; set; }
    }
}