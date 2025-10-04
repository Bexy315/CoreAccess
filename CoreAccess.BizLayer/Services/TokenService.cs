using System.Security.Claims;
using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;
using OpenIddict.Validation;

namespace CoreAccess.BizLayer.Services;

public interface ITokenService
{
    Task<ClaimsPrincipal?> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<long> RevokeUserTokensAsync(string userId, CancellationToken cancellationToken = default);
    public Claim GetClaim(ClaimsPrincipal principal, string claimType);
}
public class TokenService(OpenIddictValidationService openIddictValidationService, IOpenIddictTokenManager tokenManager) : ITokenService
{  
    public async Task<ClaimsPrincipal?> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            
            var result =  await openIddictValidationService.ValidateAccessTokenAsync(token, cancellationToken);

            return result;
        }
        catch
        {
            return null;
        }
    }

    public async Task<long> RevokeUserTokensAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await tokenManager.RevokeAsync(userId, null, "valid", null, cancellationToken);
    }

    public Claim GetClaim(ClaimsPrincipal principal, string claimType)
    {
        if (principal == null)
            throw new ArgumentNullException(nameof(principal), "ClaimsPrincipal cannot be null"); 
        
        return principal.Claims.FirstOrDefault(c => c.Type == claimType) ?? throw new InvalidOperationException($"Claim '{claimType}' not found in the principal.");
    }
}