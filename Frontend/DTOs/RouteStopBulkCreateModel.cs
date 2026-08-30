using System.ComponentModel.DataAnnotations;

namespace Frontend.DTOs
{
    public class RouteStopBulkCreateModel
    {
        [Required]
        public int BusId { get; set; }

        public List<RouteStopBulkItemModel> Stops { get; set; }= new List<RouteStopBulkItemModel>();
    }

    public class RouteStopBulkItemModel
    {
        [Required]
        [StringLength(100)]
        public string StopName { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int SeqOrder { get; set; }
    }
}