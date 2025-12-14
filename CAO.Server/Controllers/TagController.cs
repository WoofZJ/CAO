using CAO.Server.Models;
using CAO.Shared.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CAO.Server.Controllers;

[ApiController]
[Route("tag")]
public class TagController(CaoDbContext dbContext) : ControllerBase
{
    private readonly CaoDbContext _dbContext = dbContext;

    [HttpGet("list/{tag}")]
    public async Task<IActionResult> GetTagListAsync(string tag)
    {
        var tagList = await _dbContext.Blogs
            .Where(b => b.Status == BlogStatus.Published && b.Tags.Contains(tag))
            .Select(b => new TagItemResponse(
                b.Slug,
                b.Title,
                b.UpdatedAt)).ToListAsync();
        return Ok(tagList);
    }

    [HttpGet("count")]
    public async Task<IActionResult> GetTagCountListAsync()
    {
        var allTags = await _dbContext.Blogs
            .Where(b => b.Status == BlogStatus.Published)
            .Select(b => b.Tags)
            .ToListAsync();

        var tagCounts = allTags
            .SelectMany(t => t)
            .GroupBy(tag => tag)
            .Select(g => new TagCountResponse(
                g.Key,
                g.Count()))
            .ToList();
        return Ok(tagCounts);
    }
}