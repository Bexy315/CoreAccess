using System.Security.Claims;
using OpenIddict.Validation;

namespace CoreAccess.BizLayer.Services;

public interface ITokenService
{
    Task<ClaimsPrincipal?> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
    public Claim GetClaim(ClaimsPrincipal principal, string claimType);
}
public class TokenService(OpenIddictValidationService openIddictValidationService) : ITokenService
{  
    public async Task<ClaimsPrincipal?> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            return await openIddictValidationService.ValidateAccessTokenAsync(token, cancellationToken);
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