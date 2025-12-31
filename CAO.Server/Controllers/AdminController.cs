using CAO.Server.Models;
using CAO.Shared.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CAO.Server.Controllers;

[ApiController]
[Route("admin")]
public class AdminController(CaoDbContext dbContext) : ControllerBase
{
    private readonly CaoDbContext _dbContext = dbContext;

    [HttpGet("stats")]
    [Authorize]
    public async Task<IActionResult> GetAdminStatistics()
    {
        var totalPosts = await _dbContext.Blogs.CountAsync();
        var draftPosts = await _dbContext.Blogs.CountAsync(b => b.Status == BlogStatus.Draft);
        var publishedPosts = await _dbContext.Blogs.CountAsync(b => b.Status == BlogStatus.Published);
        var archivedPosts = await _dbContext.Blogs.CountAsync(b => b.Status == BlogStatus.Archived);

        var totalMessages = await _dbContext.Messages.CountAsync();
        var pendingMessages = await _dbContext.Messages.CountAsync(m => m.Status == MessageStatus.Pending);
        var approvedMessages = await _dbContext.Messages.CountAsync(m => m.Status == MessageStatus.Approved);
        var rejectedMessages = await _dbContext.Messages.CountAsync(m => m.Status == MessageStatus.Rejected);

        var statistics = new AdminStatisticsDto
        {
            TotalPosts = totalPosts,
            DraftPosts = draftPosts,
            PublishedPosts = publishedPosts,
            ArchivedPosts = archivedPosts,
            TotalMessages = totalMessages,
            PendingMessages = pendingMessages,
            ApprovedMessages = approvedMessages,
            RejectedMessages = rejectedMessages
        };

        return Ok(statistics);
    }

    [HttpGet("post/metadata/{id}")]
    [Authorize]
    public async Task<IActionResult> GetBlogEditMetadata(int id)
    {
        var blog = await _dbContext.Blogs.FindAsync(id);
        if (blog == null)
        {
            return NotFound();
        }

        var metadataDto = new BlogEditMetadataDto(
            blog.Id,
            blog.Title,
            blog.Slug,
            blog.Description,
            blog.ImageUrl,
            blog.Tags,
            (int)blog.Status,
            blog.IsRecommended,
            blog.IsSticky
        );

        return Ok(metadataDto);
    }
    [HttpPost("post/metadata")]
    [Authorize]
    public async Task<IActionResult> UpdateBlogEditMetadata([FromBody] BlogEditMetadataDto metadataDto)
    {
        Blog blog = new ();
        if (metadataDto.Id != 0)
        {
            var existingBlog =  await _dbContext.Blogs.FindAsync(metadataDto.Id);
            if (existingBlog == null)
            {
                return NotFound();
            }
            blog = existingBlog;
        }
        blog.Id = metadataDto.Id;
        blog.Title = metadataDto.Title;
        blog.Slug = metadataDto.Slug;
        if (await _dbContext.Blogs.AnyAsync(b => b.Slug == metadataDto.Slug && b.Id != metadataDto.Id))
        {
            return Ok(new BlogEditMetadataResponseDto(
                metadataDto.Id,
                false,
                true
            ));
        }
        blog.Description = metadataDto.Description;
        blog.ImageUrl = metadataDto.ImageUrl;
        blog.Tags = metadataDto.Tags;
        blog.Status = (BlogStatus)metadataDto.Status;
        if (metadataDto.IsRecommended)
        {
            blog.DisplayType |= BlogDisplayType.Recommended;
        }
        if (metadataDto.IsSticky)
        {
            blog.DisplayType |= BlogDisplayType.Sticky;
        }

        if (blog.Id == 0)
        {
            await _dbContext.Blogs.AddAsync(blog);
        }
        else
        {
            _dbContext.Blogs.Update(blog);
        }
        await _dbContext.SaveChangesAsync();
        return Ok(new BlogEditMetadataResponseDto(
            blog.Id,
            true,
            false
        ));
    }
}