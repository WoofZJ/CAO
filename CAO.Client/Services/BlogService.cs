using CAO.Shared.Dtos;

namespace CAO.Client.Services;

public class BlogService(ApiService apiService)
{
    private readonly ApiService _apiService = apiService;

    public async Task<BlogMetadataResponse?> GetBlogMetadataAsync(string slug) =>
        await _apiService.GetWithCacheAsync<BlogMetadataResponse>(
            $"blog/metadata/{slug}", $"blog_metadata_{slug}", TimeSpan.FromMinutes(15));

    public async Task<BlogHtmlResponse?> GetBlogHtmlAsync(string slug) =>
        await _apiService.GetWithCacheAsync<BlogHtmlResponse>(
            $"blog/html/{slug}", $"blog_html_{slug}", TimeSpan.FromMinutes(15));

    public async Task<List<BlogMetadataResponse>?> GetRecommendedBlogsAsync() =>
        await _apiService.GetWithCacheAsync<List<BlogMetadataResponse>>(
            "blog/recommended", "blog_recommended", TimeSpan.FromMinutes(15));
}