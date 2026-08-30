namespace Backend.DTO
{
    public class WorkerDTO
    {
        public string Name { get; set; }
        public string IqamaNo { get; set; }
        public string Nationality { get; set; }
        public string Trade { get; set; }

        public int CampId { get; set; }
        public int BusId { get; set; }
        public int BedId { get; set; }

        public int UserId { get; set; }
    }
}