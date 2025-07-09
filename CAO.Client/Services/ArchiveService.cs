using CAO.Shared.Dtos;

namespace CAO.Client.Services;

public class ArchiveService(ApiService apiService)
{
    private readonly ApiService _apiService = apiService;

    public async Task<List<ArchiveResponse>?> GetArchiveListAsync() =>
        await _apiService.GetWithCacheAsync<List<ArchiveResponse>>(
            "archive", "archive", TimeSpan.FromMinutes(15));
}