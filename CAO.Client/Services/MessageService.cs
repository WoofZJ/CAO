using System.Net.Http.Json;
using CAO.Shared.Dtos;

namespace CAO.Client.Services;

public class MessageService(ApiService apiService, StorageService storageService)
{
    private readonly ApiService _apiService = apiService;
    private readonly StorageService _storageService = storageService;

    public async Task<List<MessageGetResponse>?> GetAllMessagesAsync() =>
        await _apiService.GetAsync<List<MessageGetResponse>>("message/list");

    public async Task<List<MessageGetResponse>?> GetSelfMessagesAsync()
    {
        var visitorId = await _storageService.GetOrCreateVisitorIdAsync();
        return await _apiService.GetAsync<List<MessageGetResponse>>(
            $"message/self?VisitorId={visitorId}");
    }

    public async Task<bool> AddMessageAsync(
        string nickname, string email, string content, int avatarId)
    {
        var sessionId = await _storageService.GetOrCreateSessionIdAsync();
        var request = new MessagePostRequest(
            nickname, email, content, avatarId, sessionId);
        if (await _apiService.PostAsync("message/add", request))
        {
            OnMessageAdded?.Invoke(new MessageGetResponse(
                nickname, content, avatarId, DateTime.UtcNow, 0));
            return true;
        }
        else
        {
            return false;
        }
    }

    public event Action<MessageGetResponse>? OnMessageAdded;
}