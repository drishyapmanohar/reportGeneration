namespace api.Models;

public class ReportJob
{
    public Guid Id { get; set; }
    public string ReportType { get; set; } = "Credit Report";
    public string Status { get; set; } = "Pending";
    public string? FileUrl { get; set; }
    public string? FilePath { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsRead { get; set; } = false;
}