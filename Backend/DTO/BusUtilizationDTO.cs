namespace Backend.DTO
{
    public class BusUtilizationDTO
    {
        public int BusNo { get; set; }
        public int Capacity { get; set; }
        public int Assignedworker { get; set; }
        public decimal UtilizationRate { get; set; }

    }
}
