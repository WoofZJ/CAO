using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace CAO.Client.Services;

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

public record CommitDto(
    string Message,
    string Subject,
    int FilesChanged,
    int Insertions,
    int Deletions,
    DateTime Date
);

public record TimelineResponse(
    string Version,
    string Message,
    DateTime Date,
    List<CommitDto> Commits
);

public class TimelineObj
{
    public ExportInfo? ExportInfo { get; set; }
    public List<GitTag> Tags { get; set; } = [];
    public List<Commit> Commits { get; set; } = [];

}

public class TimelineService(NavigationManager navigationManager)
{
    private readonly NavigationManager _navigationManager = navigationManager;

    public async Task<List<TimelineResponse>?> GetTimelineItemsAsync()
    {
        using var client = new HttpClient
        {
            BaseAddress = new Uri(_navigationManager.BaseUri)
        };
        try
        {
            var timelineData = await client.GetFromJsonAsync<TimelineObj>("timeline.json");
            if (timelineData is null) return null;
            timelineData.Commits.Sort((a, b) => a.Date.CompareTo(b.Date));
            timelineData.Tags.Sort((a, b) => a.Date.CompareTo(b.Date));

            List<CommitDto> commits = [];
            List<TimelineResponse> timelineList = [];
            int tagIndex = 0;

            foreach (var commit in timelineData.Commits)
            {
                commits.Add(new CommitDto(
                    commit.Body,
                    commit.Message.Split(':')[0].Trim(),
                    commit.Stats.FilesChanged,
                    commit.Stats.Insertions,
                    commit.Stats.Deletions,
                    commit.Date
                ));

                if (tagIndex < timelineData.Tags.Count && commit.Hash == timelineData.Tags[tagIndex].Commit)
                {
                    timelineList.Add(new TimelineResponse(
                        timelineData.Tags[tagIndex].Name,
                        timelineData.Tags[tagIndex].Body,
                        timelineData.Tags[tagIndex].Date,
                        commits
                    ));
                    commits = [];
                    tagIndex++;
                }
            }

            if (commits.Count > 0)
            {
                timelineList.Add(new TimelineResponse(
                    "构建中版本",
                    "",
                    DateTime.UtcNow,
                    commits
                ));
            }
            return timelineList;
        }
        catch
        {
            return null;
        }
    }
}