using System.Net.Http.Headers;
using System.Security.Claims;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using CAO.Shared.Dtos;

namespace CAO.Client.Services;

public class CustomAuthStateProvider(IJSRuntime jsRuntime, HttpClient httpClient) : AuthenticationStateProvider
{
    private readonly IJSRuntime _jsRuntime = jsRuntime;
    private readonly HttpClient _httpClient = httpClient;
    private const string TokenKey = "authToken";

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        string? token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", TokenKey);

        if (string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt")));
    }

    public async Task<bool> Login(string password)
    {
        var authRequest = new LoginRequest(password);
        var response = await _httpClient.PostAsJsonAsync("/auth/login", authRequest);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (result != null && !string.IsNullOrEmpty(result.Token))
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", TokenKey, result.Token);
                NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
                return true;
            }
        }
        return false;
    }

    public async Task Logout()
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", TokenKey);
        _httpClient.DefaultRequestHeaders.Authorization = null;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var payload = jwt.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

        if (keyValuePairs == null) return [];

        return keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString() ?? ""));
    }

    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}
