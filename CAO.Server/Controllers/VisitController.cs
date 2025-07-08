using CAO.Server.Models;
using CAO.Shared.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CAO.Server.Controllers;

[ApiController]
[Route("visit")]
public class VisitController(CaoDbContext dbContext) : ControllerBase
{
    private readonly CaoDbContext _dbContext = dbContext;

    [HttpPost("record")]
    public async Task<IActionResult> RecordVisitAsync([FromBody] VisitRecordRequest request)
    {
        if (request is null)
        {
            return BadRequest("Visit record request cannot be null.");
        }

        var visit = new Visit
        {
            IpAddress = GetClientIpAddress(),
            UserAgent = request.UserAgent,
            Origin = request.Origin,
            Path = request.Path,
            Query = request.Query,
            Referer = request.Referer,
            VisitAt = DateTime.UtcNow,
            VisitorId = request.VisitorId,
            SessionId = request.SessionId
        };

        await _dbContext.Visits.AddAsync(visit);
        await _dbContext.SaveChangesAsync();

        return await GetVisitAsync(new VisitGetRequest(request.Path));
    }

    [HttpPost("get")]
    public async Task<IActionResult> GetVisitAsync([FromBody] VisitGetRequest request)
    {
        if (request is null)
        {
            return BadRequest("Visit get request cannot be null.");
        }

        var monthlyVisits = _dbContext.Visits
            .Where(v => v.VisitAt.Month == DateTime.UtcNow.Month &&
                        v.VisitAt.Year == DateTime.UtcNow.Year);
        int pageVisits = await _dbContext.Visits.CountAsync(v => v.Path == request.Path);
        int siteVisits = await _dbContext.Visits.CountAsync();
        int visitors = await _dbContext.Visits.Select(v => v.VisitorId).Distinct().CountAsync();
        int monthlyPageVisits = await monthlyVisits.CountAsync(v => v.Path == request.Path);
        int monthlySiteVisits = await monthlyVisits.CountAsync();
        int monthlyVisitors = await monthlyVisits.Select(v => v.VisitorId).Distinct().CountAsync();

        VisitGetResponse response = new
        (
            pageVisits,
            siteVisits,
            visitors,
            monthlyPageVisits,
            monthlySiteVisits,
            monthlyVisitors
        );

        return Ok(response);
    }

    private string GetClientIpAddress()
    {
        if (Request.Headers.TryGetValue("CF-Connecting-IP", out var cfIp)
            && !string.IsNullOrEmpty(cfIp))
        {
            return cfIp.ToString();
        }
        if (Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedIps)
            && !string.IsNullOrEmpty(forwardedIps))
        {
            return forwardedIps.ToString().Split(',')[0].Trim();
        }
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
    }
}