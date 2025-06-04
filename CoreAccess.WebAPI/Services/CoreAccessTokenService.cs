using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CoreAccess.WebAPI.Helpers;
using CoreAccess.WebAPI.Model;
using CoreAccess.WebAPI.Repositories;
using Microsoft.IdentityModel.Tokens;

namespace CoreAccess.WebAPI.Services;

public interface ICoreAccessTokenService
{
    string GenerateAccessToken(CoreUser user, string scope = "default", CancellationToken cancellationToken = default);

    Task<RefreshTokenResponse> RefreshTokenAsync(CoreRefreshTokenRequest request, CancellationToken cancellationToken = default);
    Task<RefreshToken> GenerateRefreshTokenAsync(CoreUser user, string createdByIp, CancellationToken cancellationToken = default);
    Task RecycleRefreshTokensAsync(CancellationToken cancellationToken = default);
    Task RevokeRefreshTokenAsync(string token, string revokedByIp = "0.0.0.0", CancellationToken cancellationToken = default);
    Task<CoreUser> ValidateRefreshToken(string token, CancellationToken cancellationToken = default);
    ClaimsPrincipal? ValidateToken(string token);
    public Claim GetClaim(ClaimsPrincipal principal, string claimType);
}
public class CoreAccessTokenService(
    IUserService userService,
    IRefreshTokenRepository refreshTokenRepository)
    : ICoreAccessTokenService
{
    public string GenerateAccessToken(CoreUser user, string scope = "default", CancellationToken cancellationToken = default)
    {
        if (!AppSettingsHelper.TryGet("Jwt:Secret", out string? secret, decryptIfNeeded: true))
            throw new InvalidOperationException("JWT Secret is not set in AppSettings.");
        
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
        
        if (!AppSettingsHelper.TryGet("Jwt:Issuer", out string? issuer, decryptIfNeeded: true))
            throw new InvalidOperationException("JWT Issuer is not set in AppSettings.");
        
        if (!AppSettingsHelper.TryGet("Jwt:Audience", out string? audience, decryptIfNeeded: true))
            throw new InvalidOperationException("JWT Audience is not set in AppSettings.");
        
        if (!AppSettingsHelper.TryGet("Jwt:ExpiresIn", out string? expiresIn, decryptIfNeeded: true))
            throw new InvalidOperationException("JWT ExpiresIn is not set in AppSettings.");
        
        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(int.TryParse(expiresIn, out var minutes) ? minutes : 60),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<RefreshTokenResponse> RefreshTokenAsync(CoreRefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var user = await ValidateRefreshToken(request.RefreshToken, cancellationToken);
        
        var newAccessToken = GenerateAccessToken(user, cancellationToken: cancellationToken);
        var newRefreshToken = await GenerateRefreshTokenAsync(user, request.LoginIp, cancellationToken);

        await RevokeRefreshTokenAsync(request.RefreshToken, request.LoginIp, cancellationToken);
        await RecycleRefreshTokensAsync(cancellationToken);

        return new RefreshTokenResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken.Token,
            UserId = user.Id
        };
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
        var secret = AppSettingsHelper.Get("Jwt:Secret", decryptIfNeeded: true);
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
}