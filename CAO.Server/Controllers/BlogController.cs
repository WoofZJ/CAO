using CAO.Server.Models;
using CAO.Shared.Dtos;
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
}