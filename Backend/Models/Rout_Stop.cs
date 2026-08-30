namespace Backend.Models
{
    public class Rout_Stop
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int SeqOerder { get; set; }

        public int BusId { get; set; }
        public Bus Bus { get; set; }
    }
}
