using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Calendar.Core.Entities;
using Calendar.Core.Exceptions;
using Calendar.Core.Interfaces;
using Calendar.Service.Interfaces;
using Calendar.Shared.Common;
using Calendar.Shared.DTOs.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Calendar.Service.Implementations;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public AuthService(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
    }

    public async Task<ApiResult<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        if (await _userRepository.ExistsAsync(request.Username, ct))
        {
            throw new DuplicateUsernameException("Tên đăng nhập đã tồn tại.");
        }

        if (await _userRepository.EmailExistsAsync(request.Email, ct))
        {
            throw new DuplicateUsernameException("Email đã tồn tại.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            DisplayName = request.DisplayName ?? request.Username,
            CreatedDate = DateTime.UtcNow
        };

        _userRepository.Add(user);
        await _userRepository.SaveChangesAsync(ct);

        var token = GenerateJwtToken(user);

        return ApiResult<AuthResponse>.Ok(new AuthResponse
        {
            Token = token,
            Username = user.Username,
            DisplayName = user.DisplayName,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        });
    }

    public async Task<ApiResult<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username, ct);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return ApiResult<AuthResponse>.Fail("Tên đăng nhập hoặc mật khẩu không chính xác.");
        }

        var token = GenerateJwtToken(user);

        return ApiResult<AuthResponse>.Ok(new AuthResponse
        {
            Token = token,
            Username = user.Username,
            DisplayName = user.DisplayName,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        });
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = Encoding.ASCII.GetBytes(jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key is missing"));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim("DisplayName", user.DisplayName ?? "")
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
