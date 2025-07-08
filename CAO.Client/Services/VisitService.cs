using CAO.Shared.Dtos;
using Microsoft.JSInterop;
using Microsoft.Extensions.Caching.Memory;

namespace CAO.Client.Services;

public class VisitService(
    ApiService apiService, IJSRuntime jsRuntime,
    StorageService storageService, IMemoryCache cache)
{
    private readonly ApiService _apiService = apiService;
    private readonly IJSRuntime _jsRuntime = jsRuntime;
    private readonly StorageService _storageService = storageService;
    private readonly IMemoryCache _cache = cache;

    public async Task<VisitGetResponse?> RecordVisitAsync()
    {
        var path = await _jsRuntime.InvokeAsync<string>("eval", "location.pathname");
        if (_cache.TryGetValue($"visit-{path}", out bool _))
        {
            return await GetVisitAsync();
        }

        var userAgent = await _jsRuntime.InvokeAsync<string>("eval", "navigator.userAgent");
        var origin = await _jsRuntime.InvokeAsync<string>("eval", "location.origin");
        var query = await _jsRuntime.InvokeAsync<string>("eval", "location.search");
        var referer = await _jsRuntime.InvokeAsync<string>("eval", "document.referrer");

        VisitRecordRequest request = new
        (
            userAgent,
            origin,
            path,
            query,
            referer,
            await _storageService.GetOrCreateVisitorIdAsync(),
            await _storageService.GetOrCreateSessionIdAsync()
        );

        var result = await _apiService.PostAsync<VisitRecordRequest, VisitGetResponse>(
            "visit/record", request);
        if (result is not null)
        {
            _cache.Set($"visit-{path}", true, TimeSpan.FromMinutes(15));
        }
        return result;
    }

    public async Task<VisitGetResponse?> GetVisitAsync()
    {
        string path = await _jsRuntime.InvokeAsync<string>("eval", "location.pathname");
        VisitGetRequest request = new(path);
        return await _apiService.PostAsync<VisitGetRequest, VisitGetResponse>(
            "visit/get", request);
    }
}