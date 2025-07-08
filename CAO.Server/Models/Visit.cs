namespace CAO.Server.Models;

public class Visit
{
    public int Id { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public DateTime VisitAt { get; set; } = DateTime.UtcNow;
    public string Referer { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public Guid VisitorId { get; set; } = Guid.Empty;
    public Guid SessionId { get; set; } = Guid.Empty;
}