using Backend.Models;

public class Leave
{
    public int Id { get; set; }

    public DateOnly FromeDate { get; set; }
    public DateOnly ToDate { get; set; }

    public string Status { get; set; }
    public string Reason { get; set; }
    public string? RejectionReason { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }

    public int? ApprovedBy { get; set; }
    public User? ApprovedByUser { get; set; }
}
