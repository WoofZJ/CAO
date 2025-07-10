using CAO.Server.Models;
using CAO.Shared.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CAO.Server.Controllers;

[ApiController]
[Route("message")]
public class MessageController(CaoDbContext dbContext) : ControllerBase
{
    private readonly CaoDbContext _dbContext = dbContext;

    [HttpPost("add")]
    public async Task<IActionResult> AddMessage([FromBody] MessagePostRequest request)
    {
        if (request is null)
        {
            return BadRequest("Message cannot be null.");
        }
        var visitorInfo = _dbContext.VisitorInfos
            .FirstOrDefault(v => v.SessionId == request.SessionId);
        Guid visitorId = visitorInfo!.VisitorId;

        var message = new Message
        {
            Nickname = request.Nickname,
            Email = request.Email,
            Content = request.Content,
            AvatarId = request.AvatarId,
            VisitorInfoId = visitorInfo.Id,
            CreatedAt = DateTime.UtcNow,
            Status = MessageStatus.Pending
        };

        await _dbContext.Messages.AddAsync(message);
        await _dbContext.SaveChangesAsync();

        return await GetSelfMessages(visitorId);
    }

    [HttpGet("list")]
    public async Task<IActionResult> GetAllMessages()
    {
        var messages = await _dbContext.Messages
            .Where(m => m.Status == MessageStatus.Approved)
            .ToListAsync();

        return Ok(messages);
    }

    [HttpGet("self")]
    public async Task<IActionResult> GetSelfMessages([FromQuery] Guid VisitorId)
    {
        var messages = await _dbContext.Messages
            .Where(m => _dbContext.VisitorInfos
                .Any(v => v.Id == m.VisitorInfoId && v.VisitorId == VisitorId))
            .Select(m => new MessageGetResponse(
                m.Nickname,
                m.Content,
                m.AvatarId,
                m.CreatedAt,
                (int)m.Status))
            .ToListAsync();

        return Ok(messages);
    }
}