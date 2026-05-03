using System.Net.Http.Json;
using Calendar.Shared.Common;
using Calendar.Shared.DTOs.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace Calendar.Client.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly IJSRuntime _jsRuntime;
    private const string TokenKey = "jwt_token";

    public AuthService(HttpClient httpClient, AuthenticationStateProvider authStateProvider, IJSRuntime jsRuntime)
    {
        _httpClient = httpClient;
        _authStateProvider = authStateProvider;
        _jsRuntime = jsRuntime;
    }

    public async Task<ApiResult<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);
        var result = await response.Content.ReadFromJsonAsync<ApiResult<AuthResponse>>();

        if (result != null && result.Success && result.Data != null)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", TokenKey, result.Data.Token);
            ((CustomAuthStateProvider)_authStateProvider).NotifyUserAuthentication(result.Data.Token);
        }

        return result ?? ApiResult<AuthResponse>.Fail("An error occurred connecting to the server.");
    }

    public async Task<ApiResult<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/register", request);
        var result = await response.Content.ReadFromJsonAsync<ApiResult<AuthResponse>>();
        return result ?? ApiResult<AuthResponse>.Fail("An error occurred connecting to the server.");
    }

    public async Task LogoutAsync()
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", TokenKey);
        ((CustomAuthStateProvider)_authStateProvider).NotifyUserLogout();
    }
}
