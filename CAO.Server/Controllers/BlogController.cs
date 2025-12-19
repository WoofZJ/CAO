using CAO.Server.Models;
using CAO.Shared.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CAO.Server.Controllers;

[ApiController]
[Route("blog")]
public class BlogController(CaoDbContext dbContext) : ControllerBase
{
    private readonly CaoDbContext _dbContext = dbContext;

    [HttpGet("metadata/{slug}")]
    public async Task<IActionResult> GetBlogMetadata(string slug)
    {
        var metadata = await _dbContext.Blogs
            .Where(b => b.Slug == slug && b.Status == BlogStatus.Published)
            .Select(b => new BlogMetadataResponse
            (
                b.Slug,
                b.Title,
                b.Description,
                b.ImageUrl,
                b.Tags,
                b.CreatedAt,
                b.UpdatedAt,
                b.IsRecommended,
                b.IsSticky
            )).FirstOrDefaultAsync();
        return metadata is not null ? Ok(metadata) : NotFound();
    }

    [HttpGet("html/{slug}")]
    public async Task<IActionResult> GetBlogHtml(string slug)
    {
        var html = await _dbContext.Blogs
            .Where(b => b.Slug == slug && b.Status == BlogStatus.Published)
            .Select(b => new BlogHtmlResponse(b.Slug, b.Html))
            .FirstOrDefaultAsync();
        return html is not null ? Ok(html) : NotFound();
    }

    [HttpGet("recommended")]
    public async Task<IActionResult> GetRecommendedBlogs()
    {
        var blogs = await _dbContext.Blogs
            .Where(b => b.Status == BlogStatus.Published
                && b.DisplayType.HasFlag(BlogDisplayType.Recommended))
            .Select(b => new BlogMetadataResponse
            (
                b.Slug,
                b.Title,
                b.Description,
                b.ImageUrl,
                b.Tags,
                b.CreatedAt,
                b.UpdatedAt,
                b.IsRecommended,
                b.IsSticky
            ))
            .ToListAsync();
        return Ok(blogs);
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetBlogStats()
    {
        int blogCount = await _dbContext.Blogs.CountAsync(b => b.Status == BlogStatus.Published);
        int monthlyCreatedCount = await _dbContext.Blogs
            .CountAsync(b => b.Status == BlogStatus.Published && b.CreatedAt.Month == DateTime.UtcNow.Month
                && b.CreatedAt.Year == DateTime.UtcNow.Year);
        int monthlyUpdatedCount = await _dbContext.Blogs
            .CountAsync(b => b.Status == BlogStatus.Published && b.UpdatedAt.Month == DateTime.UtcNow.Month
                && b.UpdatedAt.Year == DateTime.UtcNow.Year);

        var allTags = await _dbContext.Blogs.Select(b => b.Tags).ToListAsync();
        var flatTags = allTags.SelectMany(t => t).ToList();

        int tagCount = flatTags.Distinct().Count();
        string mostUsedTag = flatTags
            .GroupBy(t => t)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault() ?? string.Empty;
        int usedCount = await _dbContext.Blogs
            .CountAsync(b => b.Tags.Any(t => t == mostUsedTag));
        // TODO: Implement message model
        int messageCount = 0;

        var blogStats = new BlogStatsResponse
        (
            blogCount,
            monthlyCreatedCount,
            monthlyUpdatedCount,
            tagCount,
            mostUsedTag,
            usedCount,
            messageCount
        );

        return Ok(blogStats);
    }

    [HttpGet("list")]
    [Authorize]
    public async Task<IActionResult> GetBlogList()
    {
        var blogs = await _dbContext.Blogs
            .Select(b => new BlogListItemDto
            (
                b.Id,
                b.Title,
                b.Slug,
                b.Description,
                b.ImageUrl,
                b.Tags,
                b.CreatedAt,
                b.UpdatedAt,
                (int)b.Status,
                b.IsRecommended,
                b.IsSticky
            ))
            .ToListAsync();
        return Ok(blogs);
    }
}