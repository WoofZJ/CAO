using CAO.Server.Models;
using CAO.Shared.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CAO.Server.Controllers;

[ApiController]
[Route("timeline")]
public class TimelineController(CaoDbContext dbContext) : Controller
{
    private readonly CaoDbContext _dbContext = dbContext;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [HttpGet]
    public async Task<IActionResult> GetTimelineItems()
    {
        var json = await System.IO.File.ReadAllTextAsync("timeline.json");
        var obj = JsonSerializer.Deserialize<TimelineObj>(json, _jsonOptions);
        if (obj is not null)
        {
            obj.Commits.Sort((a, b) => a.Date.CompareTo(b.Date));
            obj.Tags.Sort((a, b) => a.Date.CompareTo(b.Date));
            List<CommitDto> commits = [];
            List<TimelineResponse> timelineList = [];
            int tagIndex = 0;
            foreach (var commit in obj.Commits)
            {
                commits.Add(new(
                    commit.Body,
                    commit.Message.Split(':')[0].Trim(),
                    commit.Stats.FilesChanged,
                    commit.Stats.Insertions,
                    commit.Stats.Deletions,
                    commit.Date
                ));
                if (tagIndex < obj.Tags.Count && commit.Hash == obj.Tags[tagIndex].Commit)
                {
                    timelineList.Add(new(
                        obj.Tags[tagIndex].Name,
                        obj.Tags[tagIndex].Body,
                        obj.Tags[tagIndex].Date,
                        commits
                    ));
                    commits = [];
                    ++tagIndex;
                }
            }
            if (commits.Count > 0)
            {
                timelineList.Add(new(
                    "构建中版本",
                    "",
                    DateTime.UtcNow,
                    commits
                ));
            }
            return Ok(timelineList);
        }
        return Ok(json);
    }
}