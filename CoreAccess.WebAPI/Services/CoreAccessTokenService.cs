using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CoreAccess.WebAPI.Helpers;
using CoreAccess.WebAPI.Model;
using CoreAccess.WebAPI.Repositories;
using Microsoft.IdentityModel.Tokens;

namespace CoreAccess.WebAPI.Services;

public interface ICoreAccessTokenService
{
    Task<string> GenerateAccessTokenAsync(CoreUser user, string scope = "default", CancellationToken cancellationToken = default);
    Task<RefreshToken> GenerateRefreshTokenAsync(CoreUser user, string createdByIp, CancellationToken cancellationToken = default);
    Task RecycleRefreshTokensAsync(CoreUser user, CancellationToken cancellationToken = default);
    Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    ClaimsPrincipal? ValidateToken(string token);
    public Claim GetClaim(ClaimsPrincipal principal, string claimType);
}
public class CoreAccessTokenService(
    IAppSettingsService appSettingsService,
    IUserService userService,
    IRefreshTokenRepository refreshTokenRepository)
    : ICoreAccessTokenService
{
    public async Task<string> GenerateAccessTokenAsync(CoreUser user, string scope = "default", CancellationToken cancellationToken = default)
    {
        var (secret, issuer, audience, expiresIn) = await LoadSettingsAsync(cancellationToken);

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

    public async Task<RefreshToken> GenerateRefreshTokenAsync(CoreUser user, string createdByIp, CancellationToken cancellationToken = default)
    {
        var refreshToken = new RefreshToken
        {
            Token = EncryptionKeyHelper.GenerateRandomBase64Key(),
            Expires = DateTime.UtcNow.AddDays(7),
            Created = DateTime.UtcNow,
            CreatedByIp = createdByIp,
            CoreUser = user
        };
        
        await refreshTokenRepository.UpdateOrInsertRefreshTokenAsync(refreshToken, cancellationToken);
        await refreshTokenRepository.SaveChangesAsync(cancellationToken);
        
      //  await RecycleRefreshTokensAsync(updatedUser, cancellationToken);
        
        return refreshToken;
    }
    
    public async Task RecycleRefreshTokensAsync(CoreUser user, CancellationToken cancellationToken = default)
    {
        if (user.RefreshTokens == null || !user.RefreshTokens.Any())
            return;

        var now = DateTime.UtcNow;
        user.RefreshTokens.RemoveAll(rt => rt.Expires < now);

        if (user.RefreshTokens.Count > 5)
        {
            user.RefreshTokens = user.RefreshTokens.OrderByDescending(rt => rt.Created).Take(5).ToList();
        }

        await userService.UpdateUserAsync(user.Id.ToString(), new CoreUserUpdateRequest()
        {
            RefreshTokens = user.RefreshTokens
        }, cancellationToken);
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(refreshToken))
            throw new ArgumentNullException(nameof(refreshToken), "Refresh token cannot be null or empty");

        var user = await userService.GetUserByRefreshTokenAsync(refreshToken, cancellationToken);
        if (user == null)
            throw new InvalidOperationException("User not found for the provided refresh token");

        user.RefreshTokens?.RemoveAll(rt => rt.Token == refreshToken);
        
        await userService.UpdateUserAsync(user.Id.ToString(), new CoreUserUpdateRequest()
        {
            RefreshTokens = user.RefreshTokens
        }, cancellationToken);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        var secret = appSettingsService.GetDecrypted("Jwt:Secret").Result;
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
    
    private async Task<(string Secret, string Issuer, string Audience, int ExpiresIn)> LoadSettingsAsync(CancellationToken cancellationToken = default)
    {
        await EnsureJwtSettingsInitializedAsync(cancellationToken);
        
        var secret = await appSettingsService.GetDecrypted("Jwt:Secret");
        var issuer = await appSettingsService.GetDecrypted("Jwt:Issuer");
        var audience = await appSettingsService.GetDecrypted("Jwt:Audience");
        var expiresIn = int.TryParse(await appSettingsService.GetDecrypted("Jwt:ExpiresIn"), out var val) ? val : 60;

        return (secret, issuer, audience, expiresIn);
    }

    private async Task EnsureJwtSettingsInitializedAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(await appSettingsService.GetDecrypted("Jwt:Secret")))
        {
            var secret = EncryptionKeyHelper.GenerateRandomBase64Key();
            await appSettingsService.SetEncrypted("Jwt:Secret", secret);
        }

        if (await appSettingsService.GetDecrypted("Jwt:Issuer") == null)
            await appSettingsService.SetEncrypted("Jwt:Issuer", "coreaccess");

        if (await appSettingsService.GetDecrypted("Jwt:Audience") == null)
            await appSettingsService.SetEncrypted("Jwt:Audience", "coreaccess-client");

        if (await appSettingsService.GetDecrypted("Jwt:ExpiresIn") == null)
            await appSettingsService.SetEncrypted("Jwt:ExpiresIn", "60");
    }
}