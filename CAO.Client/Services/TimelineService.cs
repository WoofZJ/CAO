using CAO.Shared.Dtos;

namespace CAO.Client.Services;

public class TimelineService(ApiService apiService)
{
    private readonly ApiService _apiService = apiService;

    public async Task<List<TimelineResponse>?> GetTimelineItemsAsync() =>
        await _apiService.GetWithCacheAsync<List<TimelineResponse>>(
            "timeline", "timeline_items", TimeSpan.FromMinutes(15));
}