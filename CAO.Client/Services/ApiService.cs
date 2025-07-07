using System.Net.Http.Json;
using Microsoft.Extensions.Caching.Memory;

namespace CAO.Client.Services;

public class ApiService(HttpClient httpClient, IMemoryCache cache)
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly IMemoryCache _cache = cache;

    public async Task<T?> GetAsync<T>(string endpoint) where T : class
    {
        var response = await _httpClient.GetAsync(endpoint);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<T>();
        }
        return null;
    }

    public async Task<T?> GetWithCacheAsync<T>(string endpoint, string cacheKey, TimeSpan duration)
        where T : class
    {
        if (_cache.TryGetValue(cacheKey, out T? cachedData))
        {
            return cachedData;
        }
        var data = await GetAsync<T>(endpoint);
        if (data != null)
        {
            _cache.Set(cacheKey, data, duration);
            return data;
        }
        return null;
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
        where TRequest : class where TResponse : class
    {
        var response = await _httpClient.PostAsJsonAsync(endpoint, data);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<TResponse>();
        }
        return null;
    }
}