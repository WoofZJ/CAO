using CAO.Shared.Dtos;

namespace CAO.Client.Services;

public class AdminService(ApiService apiService)
{
    private readonly ApiService _apiService = apiService;

    public async Task<AdminStatisticsDto?> GetAdminStatisticsAsync() =>
        await _apiService.GetAsync<AdminStatisticsDto>("admin/stats");

    public async Task<BlogEditMetadataDto?> GetBlogEditMetadataAsync(int id) =>
        await _apiService.GetAsync<BlogEditMetadataDto>($"admin/post/metadata/{id}");

    public async Task<BlogEditMetadataResponseDto?> UpdateBlogEditMetadataAsync(BlogEditMetadataDto metadataDto) =>
        await _apiService.PostAsync<BlogEditMetadataDto, BlogEditMetadataResponseDto>("admin/post/metadata", metadataDto);

    public async Task<BlogEditMarkdownDto?> GetBlogEditMarkdownAsync(int id) =>
        await _apiService.GetAsync<BlogEditMarkdownDto>($"admin/post/markdown/{id}");
    
    public async Task<BlogEditMarkdownResponseDto?> UpdateBlogEditMarkdownAsync(BlogEditMarkdownDto markdownDto) =>
        await _apiService.PostAsync<BlogEditMarkdownDto, BlogEditMarkdownResponseDto>("admin/post/markdown", markdownDto);
}