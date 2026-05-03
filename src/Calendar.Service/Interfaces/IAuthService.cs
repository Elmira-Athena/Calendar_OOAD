using Calendar.Shared.Common;
using Calendar.Shared.DTOs.Auth;

namespace Calendar.Service.Interfaces;

public interface IAuthService
{
    Task<ApiResult<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<ApiResult<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default);
}
