using CAO.Server.Models;
using CAO.Shared.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CAO.Server.Controllers;

[ApiController]
[Route("archive")]
public class ArchiveController(CaoDbContext dbContext) : ControllerBase
{
    private readonly CaoDbContext _dbContext = dbContext;

    [HttpGet]
    public async Task<IActionResult> GetArchiveListAsync()
    {
        var archiveList = await _dbContext.Blogs
            .Where(b => b.Status == BlogStatus.Published)
            .Select(b => new ArchiveResponse(
                b.Slug,
                b.Title,
                b.CreatedAt,
                b.UpdatedAt))
            .ToListAsync();
        return Ok(archiveList);
    }
}