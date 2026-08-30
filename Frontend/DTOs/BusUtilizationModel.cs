namespace Frontend.DTOs
{
    public class BusUtilizationModel
    {
        public int BusNo { get; set; }
        public int Capacity { get; set; }
        public int Assignedworker { get; set; }
        public decimal UtilizationRate { get; set; }
    }
}
