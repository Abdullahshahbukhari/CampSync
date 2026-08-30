namespace Backend.DTO
{
    public class RouteStopResponseDTO
    {
        public int Id { get; set; }

        public string StopName { get; set; }

        public int SeqOrder { get; set; }

        public int BusId { get; set; }

        public int BusNo { get; set; }
    }
}