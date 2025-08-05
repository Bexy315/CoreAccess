using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CoreAccess.WebAPI.Helpers;
using CoreAccess.WebAPI.Model;
using CoreAccess.WebAPI.Repositories;
using Microsoft.IdentityModel.Tokens;

namespace CoreAccess.WebAPI.Services;

public interface ITokenService
{
    string GenerateAccessToken(User user, string scope = "default", CancellationToken cancellationToken = default);
    ClaimsPrincipal? ValidateToken(string token);
    public Claim GetClaim(ClaimsPrincipal principal, string claimType);
}
public class TokenService(
    IAppSettingsService appSettingsService,
    IUserService userService)
    : ITokenService
{  
    private static JwtSecurityTokenHandler tokenHandler = new();
    public string GenerateAccessToken(User user, string scope = "default", CancellationToken cancellationToken = default)
    {
        if (!appSettingsService.TryGet(AppSettingsKeys.JwtSecretKey, out string? secret, decryptIfNeeded: true))
            throw new InvalidOperationException("JWT Secret is not set in AppSettings.");
        
        var key = new SymmetricSecurityKey(Convert.FromBase64String(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(AccessClaimType.TokenId, Guid.NewGuid().ToString()),
            new(AccessClaimType.UserId, user.Id.ToString()),
            new(AccessClaimType.UserName, user.Username)
        };
        if (!string.IsNullOrEmpty(scope))
        {
            claims.Add(new Claim("scope", scope));
        }
        
        foreach (var role in user.Roles)
        {
            claims.Add(new Claim(AccessClaimType.Role, role.Name));
        }
        
        
        
        if (!appSettingsService.TryGet(AppSettingsKeys.JwtIssuer, out string? issuer, decryptIfNeeded: true))
            throw new InvalidOperationException("JWT Issuer is not set in AppSettings.");
        
        if (!appSettingsService.TryGet(AppSettingsKeys.JwtAudience, out string? audience, decryptIfNeeded: true))
            throw new InvalidOperationException("JWT Audience is not set in AppSettings.");
        
        if (!appSettingsService.TryGet(AppSettingsKeys.JwtExpiresIn, out string? expiresIn, decryptIfNeeded: true))
            throw new InvalidOperationException("JWT ExpiresIn is not set in AppSettings.");
        
        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(int.TryParse(expiresIn, out var minutes) ? minutes : 60),
            signingCredentials: creds
        );

        return tokenHandler.WriteToken(token);
    }
    public ClaimsPrincipal? ValidateToken(string token)
    {
        var secret = appSettingsService.Get(AppSettingsKeys.JwtSecretKey, decryptIfNeeded: true);
        if (string.IsNullOrEmpty(secret)) return null;
        
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