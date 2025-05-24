using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CoreAccess.WebAPI.Helpers;
using CoreAccess.WebAPI.Model;
using Microsoft.IdentityModel.Tokens;

namespace CoreAccess.WebAPI.Services;

public interface ICoreAccessTokenService
{
    Task<string> GenerateAccessTokenAsync(CoreUser user, string scope = "default");
    Task<RefreshToken> GenerateRefreshTokenAsync(CoreUser user, string createdByIp);
    Task RecycleRefreshTokensAsync(CoreUser user);
    Task RevokeRefreshTokenAsync(string refreshToken);
    ClaimsPrincipal? ValidateToken(string token);
    public Claim GetClaim(ClaimsPrincipal principal, string claimType);
}
public class CoreAccessTokenService : ICoreAccessTokenService
{
    private readonly IAppSettingsService _appSettingsService;
    private readonly IUserService _userService;

    public CoreAccessTokenService(IAppSettingsService appSettingsService, IUserService userService)
    {
        _appSettingsService = appSettingsService;
        _userService = userService;
        EnsureJwtSettingsInitializedAsync().GetAwaiter().GetResult();
    }

    public async Task<string> GenerateAccessTokenAsync(CoreUser user, string scope = "default")
    {
        var (secret, issuer, audience, expiresIn) = await LoadSettingsAsync();

        var key = new SymmetricSecurityKey(Convert.FromBase64String(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(CoreAccessClaimType.UserId, user.Id.ToString()),
            new(CoreAccessClaimType.UserName, user.Username),
            new(CoreAccessClaimType.TokenId, Guid.NewGuid().ToString())
        };

        foreach (var role in user.Roles)
        {
            claims.Add(new Claim(CoreAccessClaimType.Role, role.Name));
        }

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(expiresIn),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<RefreshToken> GenerateRefreshTokenAsync(CoreUser user, string createdByIp)
    {
        var refreshToken = new RefreshToken
        {
            Token = EncryptionKeyHelper.GenerateRandomBase64Key(),
            Expires = DateTime.UtcNow.AddDays(7),
            Created = DateTime.UtcNow,
            CreatedByIp = createdByIp
        };

        user.RefreshTokens ??= new List<RefreshToken>();
        user.RefreshTokens.Add(refreshToken);

        await _userService.UpdateUserAsync(user.Id.ToString() ,new CoreUserUpdateRequest()
        {
            RefreshTokens = user.RefreshTokens
        });
        
        await RecycleRefreshTokensAsync(user);
        
        return refreshToken;
    }
    
    public async Task RecycleRefreshTokensAsync(CoreUser user)
    {
        if (user.RefreshTokens == null || !user.RefreshTokens.Any())
            return;

        var now = DateTime.UtcNow;
        user.RefreshTokens.RemoveAll(rt => rt.Expires < now);

        if (user.RefreshTokens.Count > 5)
        {
            user.RefreshTokens = user.RefreshTokens.OrderByDescending(rt => rt.Created).Take(5).ToList();
        }

        await _userService.UpdateUserAsync(user.Id.ToString(), new CoreUserUpdateRequest()
        {
            RefreshTokens = user.RefreshTokens
        });
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
            throw new ArgumentNullException(nameof(refreshToken), "Refresh token cannot be null or empty");

        var user = await _userService.GetUserByRefreshTokenAsync(refreshToken);
        if (user == null)
            throw new InvalidOperationException("User not found for the provided refresh token");

        user.RefreshTokens?.RemoveAll(rt => rt.Token == refreshToken);
        
        await _userService.UpdateUserAsync(user.Id.ToString(), new CoreUserUpdateRequest()
        {
            RefreshTokens = user.RefreshTokens
        });
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        var secret = _appSettingsService.GetDecrypted("Jwt:Secret").Result;
        if (string.IsNullOrEmpty(secret)) return null;

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Convert.FromBase64String(secret));

        try
        {
            return tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                IssuerSigningKey = key,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.FromSeconds(30)
            }, out _);
        }
        catch
        {
            return null;
        }
    }
    
    public Claim GetClaim(ClaimsPrincipal principal, string claimType)
    {
        if (principal == null)
            throw new ArgumentNullException(nameof(principal), "ClaimsPrincipal cannot be null"); 
        
        return principal.Claims.FirstOrDefault(c => c.Type == claimType) ?? throw new InvalidOperationException($"Claim '{claimType}' not found in the principal.");
    }
    
    private async Task<(string Secret, string Issuer, string Audience, int ExpiresIn)> LoadSettingsAsync()
    {
        var secret = await _appSettingsService.GetDecrypted("Jwt:Secret");
        var issuer = await _appSettingsService.GetDecrypted("Jwt:Issuer");
        var audience = await _appSettingsService.GetDecrypted("Jwt:Audience");
        var expiresIn = int.TryParse(await _appSettingsService.GetDecrypted("Jwt:ExpiresIn"), out var val) ? val : 60;

        return (secret, issuer, audience, expiresIn);
    }

    private async Task EnsureJwtSettingsInitializedAsync()
    {
        if (string.IsNullOrWhiteSpace(await _appSettingsService.GetDecrypted("Jwt:Secret")))
        {
            var secret = EncryptionKeyHelper.GenerateRandomBase64Key();
            await _appSettingsService.SetEncrypted("Jwt:Secret", secret);
        }

        if (await _appSettingsService.GetDecrypted("Jwt:Issuer") == null)
            await _appSettingsService.SetEncrypted("Jwt:Issuer", "coreaccess");

        if (await _appSettingsService.GetDecrypted("Jwt:Audience") == null)
            await _appSettingsService.SetEncrypted("Jwt:Audience", "coreaccess-client");

        if (await _appSettingsService.GetDecrypted("Jwt:ExpiresIn") == null)
            await _appSettingsService.SetEncrypted("Jwt:ExpiresIn", "60");
    }
}