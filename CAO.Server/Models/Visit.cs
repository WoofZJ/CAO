namespace CAO.Server.Models;

public class Visit
{
    public int Id { get; set; }
    public DateTime VisitAt { get; set; } = DateTime.UtcNow;
    public string Referer { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public int VisitorInfoId { get; set; }
}