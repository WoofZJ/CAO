using CAO.Shared.Dtos;

namespace CAO.Client.Services;

public class AdminService(ApiService apiService)
{
    private readonly ApiService _apiService = apiService;

    public async Task<AdminStatisticsDto?> GetAdminStatisticsAsync() =>
        await _apiService.GetAsync<AdminStatisticsDto>("admin/stats");
}