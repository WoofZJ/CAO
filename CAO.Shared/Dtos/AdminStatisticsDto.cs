namespace CAO.Shared.Dtos;

public record AdminStatisticsDto
{
    public int TotalPosts { get; init; }
    public int DraftPosts { get; init; }
    public int PublishedPosts { get; init; }
    public int ArchivedPosts { get; init; }
    public int TotalMessages { get; init; }
    public int PendingMessages { get; init; }
    public int ApprovedMessages { get; init; }
    public int RejectedMessages { get; init; }
}