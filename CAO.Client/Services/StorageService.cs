using Microsoft.JSInterop;

namespace CAO.Client.Services;

public class StorageService(IJSRuntime jsRuntime)
{
    private readonly IJSRuntime _jsRuntime = jsRuntime;

    public async Task<string?> GetStorageItemAsync(string key) =>
        await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);

    public async Task SetStorageItemAsync(string key, string value) =>
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value);

    public async Task RemoveStorageItemAsync(string key) =>
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);

    public async Task<string?> GetSessionItemAsync(string key) =>
        await _jsRuntime.InvokeAsync<string?>("sessionStorage.getItem", key);

    public async Task SetSessionItemAsync(string key, string value) =>
        await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", key, value);

    public async Task RemoveSessionItemAsync(string key) =>
        await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", key);

    public async Task<Guid> GetOrCreateVisitorIdAsync()
    {
        var visitorIdStr = await GetStorageItemAsync("visitorId");
        if (string.IsNullOrEmpty(visitorIdStr))
        {
            var newVisitorId = Guid.NewGuid();
            await SetStorageItemAsync("visitorId", newVisitorId.ToString());
            return newVisitorId;
        }
        return Guid.Parse(visitorIdStr);
    }

    public async Task<Guid> GetOrCreateSessionIdAsync()
    {
        var sessionIdStr = await GetSessionItemAsync("sessionId");
        if (string.IsNullOrEmpty(sessionIdStr))
        {
            var newSessionId = Guid.NewGuid();
            await SetSessionItemAsync("sessionId", newSessionId.ToString());
            return newSessionId;
        }
        return Guid.Parse(sessionIdStr);
    }
}