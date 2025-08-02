using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CoreAccess.WebAPI.Model;
using CoreAccess.WebAPI.Repositories;
using CoreAccess.WebAPI.Services;
using Microsoft.IdentityModel.Tokens;

namespace CoreAccess.Tests;

public class TokenServiceTests
{
    private readonly Mock<IAppSettingsService> _appSettingsService = new();
    private readonly Mock<IUserService> _userService = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepository = new();
    private readonly TokenService _tokenService;

    public TokenServiceTests()
    {
        _tokenService = new TokenService(
            _appSettingsService.Object,
            _userService.Object,
            _refreshTokenRepository.Object
        );
    }

    [Fact]
    public void GenerateAccessToken_Returns_ValidJwt()
    {
        // Arrange
        var secretKey = Convert.ToBase64String(Encoding.UTF8.GetBytes("supersecretkey12345678901234567890")); // must be 256-bit
        _appSettingsService.Setup(x => x.TryGet(AppSettingsKeys.JwtSecretKey, out secretKey, true)).Returns(true);
        _appSettingsService.Setup(x => x.TryGet(AppSettingsKeys.JwtIssuer, out It.Ref<string>.IsAny, true))
            .Callback(new TryGetCallback((string key, out string? value, bool decrypt) => value = "issuer")).Returns(true);
        _appSettingsService.Setup(x => x.TryGet(AppSettingsKeys.JwtAudience, out It.Ref<string>.IsAny, true))
            .Callback(new TryGetCallback((string key, out string? value, bool decrypt) => value = "aud")).Returns(true);
        _appSettingsService.Setup(x => x.TryGet(AppSettingsKeys.JwtExpiresIn, out It.Ref<string>.IsAny, true))
            .Callback(new TryGetCallback((string key, out string? value, bool decrypt) => value = "60")).Returns(true);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Roles = new List<Role> { new Role { Name = "Admin" } }
        };

        // Act
        var token = _tokenService.GenerateAccessToken(user);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(token));
        var handler = new JwtSecurityTokenHandler();
        Assert.True(handler.CanReadToken(token));
    }

    [Fact]
    public void ValidateToken_WithValidToken_ReturnsClaimsPrincipal()
    {
        // Arrange
        var key = Convert.ToBase64String(Encoding.UTF8.GetBytes("supersecretkey12345678901234567890"));
        _appSettingsService.Setup(x => x.Get(AppSettingsKeys.JwtSecretKey, true)).Returns(key);

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(new SecurityTokenDescriptor
        {
            Expires = DateTime.UtcNow.AddMinutes(5),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Convert.FromBase64String(key)),
                SecurityAlgorithms.HmacSha256
            )
        });

        var tokenString = handler.WriteToken(token);

        // Act
        var result = _tokenService.ValidateToken(tokenString);

        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<ClaimsPrincipal>(result);
    }

    [Fact]
    public void GetClaim_ReturnsExpectedClaim()
    {
        // Arrange
        var claim = new Claim("test-type", "test-value");
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { claim }));

        // Act
        var result = _tokenService.GetClaim(principal, "test-type");

        // Assert
        Assert.Equal("test-value", result.Value);
    }

    [Fact]
    public async Task ValidateRefreshToken_WithInvalidToken_Throws()
    {
        // Arrange
        _refreshTokenRepository.Setup(r => r.GetRefreshTokenAsync("invalid-token",null, null, false,  It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _tokenService.ValidateRefreshToken("invalid-token"));
    }

    [Fact]
    public async Task RefreshTokenAsync_Returns_NewTokens()
    {
        // Arrange
        var token = "valid-refresh-token";
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "test",
            Roles = new List<Role>()
        };

        _refreshTokenRepository.Setup(r => r.GetRefreshTokenAsync(token, null, null, false,  It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RefreshToken { Token = token, Expires = DateTime.UtcNow + TimeSpan.FromDays(1), User = user });

        _userService.Setup(u => u.GetUserByRefreshTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _appSettingsService.Setup(x => x.TryGet(It.IsAny<string>(), out It.Ref<string>.IsAny, true))
            .Callback(new TryGetCallback((string k, out string? v, bool d) =>
            {
                v = k switch
                {
                    AppSettingsKeys.JwtSecretKey => Convert.ToBase64String(Encoding.UTF8.GetBytes("supersecretkey12345678901234567890")),
                    AppSettingsKeys.JwtIssuer => "issuer",
                    AppSettingsKeys.JwtAudience => "audience",
                    AppSettingsKeys.JwtExpiresIn => "60",
                    _ => ""
                };
            })).Returns(true);

        _refreshTokenRepository.Setup(r => r.UpdateOrInsertRefreshTokenAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()));
        _refreshTokenRepository
            .Setup(r => r.GetAllRefreshTokenAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RefreshToken>()); // <-- statt null


        // Act
        var response = await _tokenService.RefreshTokenAsync(new RefreshTokenRequest
        {
            RefreshToken = token,
            LoginIp = "127.0.0.1"
        });

        // Assert
        Assert.NotNull(response);
        Assert.Equal(user.Id, response.UserId);
        Assert.False(string.IsNullOrWhiteSpace(response.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(response.RefreshToken));
    }

    // Delegate to simulate out parameters in Moq callbacks
    private delegate void TryGetCallback(string key, out string? value, bool decrypt);
}
