using CAO.Shared.Dtos;
using Microsoft.JSInterop;

namespace CAO.Client.Services;

public class VisitService(ApiService apiService, IJSRuntime jsRuntime, StorageService storageService)
{
    private readonly ApiService _apiService = apiService;
    private readonly IJSRuntime _jsRuntime = jsRuntime;
    private readonly StorageService _storageService = storageService;

    public async Task<VisitGetResponse?> RecordVisitAsync()
    {
        var userAgent = await _jsRuntime.InvokeAsync<string>("eval", "navigator.userAgent");
        var origin = await _jsRuntime.InvokeAsync<string>("eval", "location.origin");
        var path = await _jsRuntime.InvokeAsync<string>("eval", "location.pathname");
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

        return await _apiService.PostAsync<VisitRecordRequest, VisitGetResponse>(
            "visit/record", request);
    }

    public async Task<VisitGetResponse?> GetVisitAsync()
    {
        string path = await _jsRuntime.InvokeAsync<string>("location.pathname");
        VisitGetRequest request = new(path);
        return await _apiService.PostAsync<VisitGetRequest, VisitGetResponse>(
            "visit/get", request);
    }
}