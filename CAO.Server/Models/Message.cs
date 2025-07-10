namespace CAO.Server.Models;

public enum MessageStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}

public class Message
{
    public int Id { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int AvatarId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int VisitorInfoId { get; set; }
    public MessageStatus Status { get; set; } = MessageStatus.Pending;
}