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
}