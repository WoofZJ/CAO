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

        int? visitorInfoId = await _dbContext.VisitorInfos
            .FirstOrDefaultAsync(v => v.SessionId == request.SessionId)
            .ContinueWith(t => t.Result?.Id);
        
        if (visitorInfoId is null)
        {
            var visitorInfo = new VisitorInfo
            {
                VisitorId = request.VisitorId,
                SessionId = request.SessionId,
                UserAgent = request.UserAgent,
                IpAddress = GetClientIpAddress()
            };
            await _dbContext.VisitorInfos.AddAsync(visitorInfo);
            try
            {
                await _dbContext.SaveChangesAsync();
            }
            finally
            {
                // finally is in case of duplicate session
                visitorInfoId = await _dbContext.VisitorInfos
                    .FirstOrDefaultAsync(v => v.VisitorId == request.VisitorId && v.SessionId == request.SessionId)
                    .ContinueWith(t => t.Result?.Id);
            }
        }

        var visit = new Visit
        {
            Path = request.Path,
            Query = request.Query,
            Referer = request.Referer,
            VisitAt = DateTime.UtcNow,
            VisitorInfoId = visitorInfoId.Value
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
        int visitors = await _dbContext.VisitorInfos.Select(v => v.VisitorId).Distinct().CountAsync();
        int monthlyPageVisits = await monthlyVisits.CountAsync(v => v.Path == request.Path);
        int monthlySiteVisits = await monthlyVisits.CountAsync();
        int monthlyVisitors = await _dbContext.VisitorInfos
            .Where(v => monthlyVisits.Any(mv => mv.VisitorInfoId == v.Id))
            .Select(v => v.VisitorId).Distinct().CountAsync();

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