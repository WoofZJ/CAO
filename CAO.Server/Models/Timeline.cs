namespace CAO.Server.Models;

public record CommitStats(
    int FilesChanged,
    int Insertions,
    int Deletions
);

public record Commit(
    string Message,
    string Body,
    string Hash,
    DateTime Date,
    CommitStats Stats
);

public record GitTag(
    string Name,
    string Message,
    string Body,
    string Commit,
    DateTime Date
);

public record ExportInfo(
    DateTime ExportDate,
    int TotalCommits,
    int TotalTags
);

public class TimelineObj
{
    public ExportInfo? ExportInfo { get; set; }
    public List<GitTag> Tags { get; set; } = [];
    public List<Commit> Commits { get; set; } = [];

}