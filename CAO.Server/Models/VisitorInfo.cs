using Microsoft.EntityFrameworkCore;

namespace CAO.Server.Models;

[Index(nameof(SessionId), IsUnique = true)]
public class VisitorInfo
{
    public int Id { get; set; }
    public Guid VisitorId { get; set; } = Guid.Empty;
    public Guid SessionId { get; set; } = Guid.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
}