using CAO.Shared.Dtos;

namespace CAO.Client.Services;

public class TagService(ApiService apiService)
{
    private readonly ApiService _apiService = apiService;

    public async Task<List<TagItemResponse>?> GetTagListAsync(string tag) =>
        await _apiService.GetWithCacheAsync<List<TagItemResponse>>(
            $"tag/list/{tag}", $"tag_list_{tag}", TimeSpan.FromMinutes(15));

    public async Task<List<TagCountResponse>?> GetTagCountListAsync() =>
        await _apiService.GetWithCacheAsync<List<TagCountResponse>>(
            "tag/count", "tag_count", TimeSpan.FromMinutes(15));
}