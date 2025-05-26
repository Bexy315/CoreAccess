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
    Task RecycleRefreshTokensAsync(CancellationToken cancellationToken = default);
    Task RevokeRefreshTokenAsync(string token, string revokedByIp = "0.0.0.0", CancellationToken cancellationToken = default);
    Task<CoreUser> ValidateRefreshToken(string token, CancellationToken cancellationToken = default);

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

        string roles = "";
        string permissions = "";
        
        foreach (var role in user.Roles)
        {
            roles += $"{role.Name},";

            if (role.Permissions != null && role.Permissions.Any())
            {
                foreach (var permission in role.Permissions)
                {
                    
                    permissions += $"{permission},";
                }   
            }
        }
        claims.Add(new Claim(CoreAccessClaimType.Roles, roles));
        claims.Add(new Claim(CoreAccessClaimType.Permissions, permissions));

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
            Token = SecureKeyHelper.GenerateRandomBase64Key(),
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
    
    public async Task RecycleRefreshTokensAsync(CancellationToken cancellationToken = default)
    {
        var allTokens = await refreshTokenRepository.GetAllRefreshTokenAsync(cancellationToken);

        var groupedTokens = allTokens
            .Where(t => !t.IsActive)
            .GroupBy(t => t.CoreUser.Id);

        foreach (var group in groupedTokens)
        {
            var inactiveTokens = group.OrderByDescending(t => t.Created).Skip(5).ToList();
            foreach (var token in inactiveTokens)
            {
                await refreshTokenRepository.DeleteRefreshTokenAsync(token.Token, cancellationToken);
            }
        }

        await refreshTokenRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokeRefreshTokenAsync(string token, string revokedByIp = "0.0.0.0", CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(token))
            throw new ArgumentNullException(nameof(token), "Refresh token cannot be null or empty");

        var refreshToken = await refreshTokenRepository.GetRefreshTokenAsync(token, cancellationToken);
        if (refreshToken == null)
            throw new InvalidOperationException("Refresh Token not found");

        refreshToken.Revoked = DateTime.UtcNow;
        refreshToken.RevokedByIp = revokedByIp;
        
        await refreshTokenRepository.UpdateOrInsertRefreshTokenAsync(refreshToken, cancellationToken);
        await refreshTokenRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<CoreUser> ValidateRefreshToken(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(token))
            throw new ArgumentNullException(nameof(token), "Refresh token cannot be null or empty");

        var refreshToken = await refreshTokenRepository.GetRefreshTokenAsync(token, cancellationToken);
        if (refreshToken == null)
            throw new UnauthorizedAccessException("Refresh Token not found");

        if (!refreshToken.IsActive)
            throw new UnauthorizedAccessException("Refresh Token is not active");

        var user = await userService.GetUserByRefreshTokenAsync(token, cancellationToken);
        
        if(user == null)
            throw new InvalidOperationException("User associated with the refresh token not found");
        
        
        return user;
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
            var secret = SecureKeyHelper.GenerateRandomBase64Key();
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