using System.Security.Claims;
using CoreAccess.BizLayer.Services;
using CoreAccess.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace CoreAccess.WebAPI.Controllers;

[Controller]
public class AuthController(IUserService userService, IOpenIddictService openIddictService, ITokenService tokenService, ISettingsService settingsService) : ControllerBase
{
    [HttpPost("~/connect/token")]
    [IgnoreAntiforgeryToken, Produces("application/json")]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest();
        if (request == null)
            throw new InvalidOperationException("OpenIddict request is null.");
        
        if (request.IsPasswordGrantType())
        {
            var user = await userService.GetUserByUsernameAsync(request.Username);
            
            if (user == null || !await userService.ValidateCredentialsByUsernameAsync(user.Username, request.Password))
            {
                return Forbid();
            }
            
            var claims = await openIddictService.GetUserClaims(user);
            
            foreach (var claim in claims)
            {
                claim.SetDestinations(OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken, OpenIddictConstants.Destinations.IssuedToken);
            }

            var identity = new ClaimsIdentity(claims, TokenValidationParameters.DefaultAuthenticationType);
            var principal = new ClaimsPrincipal(identity);
            
            var tokenLifetime = await settingsService.GetTokenLifetimeAsync();
            principal.SetAccessTokenLifetime(TimeSpan.FromSeconds(tokenLifetime));

            principal.SetScopes(OpenIddictConstants.Scopes.OpenId, OpenIddictConstants.Scopes.OfflineAccess);
            principal.SetResources("coreaccess-api");

            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        if (request.IsRefreshTokenGrantType())
        {
            var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            var principal = result.Principal;
            if (principal == null)
                return Forbid();
            
            var tokenLifetime = await settingsService.GetTokenLifetimeAsync();
            principal.SetAccessTokenLifetime(TimeSpan.FromSeconds(tokenLifetime));
            
            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        if (request.IsAuthorizationCodeGrantType())
        {       
            var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            var principal = result.Principal;
            if (principal is null)
                return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            // Optional: Revalidate/rehydrate (User deaktiviert? Rollen geändert?)
            var userId = principal.GetClaim(OpenIddictConstants.Claims.Subject);
            
            var user = await userService.GetUserByIdAsync(userId);
            
            if (user is null)
            {
                return Forbid(
                    new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "Account no longer exists."
                    }),
                    OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
                );
            }
            
            var freshClaims = await openIddictService.GetUserClaims(user);
            foreach (var c in freshClaims)
                c.SetDestinations(OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken);

            var freshIdentity = new ClaimsIdentity(TokenValidationParameters.DefaultAuthenticationType);
            freshIdentity.AddClaims(freshClaims);
            var freshPrincipal = new ClaimsPrincipal(freshIdentity);
            
            var tokenLifetime = await settingsService.GetTokenLifetimeAsync();
            freshPrincipal.SetAccessTokenLifetime(TimeSpan.FromSeconds(tokenLifetime));
           
            freshPrincipal.SetScopes(result.Principal!.GetScopes());
            freshPrincipal.SetResources("coreaccess-api");

            return SignIn(freshPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }


        return BadRequest("Unsupported grant type.");
    }
    
    [HttpGet("~/connect/authorize")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Authorize()
    {
        var request = HttpContext.GetOpenIddictServerRequest()
                      ?? throw new InvalidOperationException("Missing OIDC request.");

        // Wenn der User nicht eingeloggt ist → Redirect zur Login Page
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return Challenge("Cookies");
        }
        var userId = User.FindFirstValue(OpenIddictConstants.Claims.Subject);
        
        var user = await userService.GetUserByIdAsync(userId);
        
        var claims = await openIddictService.GetUserClaims(user);
        var identity = new ClaimsIdentity(TokenValidationParameters.DefaultAuthenticationType);
        identity.AddClaims(claims);

        // Scopes & Ressourcen übernehmen
        identity.SetScopes(request.GetScopes());

        var principal = new ClaimsPrincipal(identity);

        // Auth-Response an OpenIddict weiterreichen
        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }
    
    [HttpGet("~/connect/userinfo"), HttpPost("~/connect/userinfo")]
    [IgnoreAntiforgeryToken, Produces("application/json")]
    public async Task<IActionResult> UserInfo()
    {
        var request = HttpContext.GetOpenIddictServerRequest();
        
        if (request == null || request.AccessToken == null)
            throw new InvalidOperationException("OpenIddict request is null.");
        
        var claimsPrincipal = await tokenService.ValidateTokenAsync(request.AccessToken);
        
        var userId = claimsPrincipal.Claims.FirstOrDefault(x => x.Type == OpenIddictConstants.Claims.Subject);
        
        if (userId == null)
        {
            return Challenge(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidToken,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The access token is invalid."
                }));
        }
        
        var user = await userService.GetUserByIdAsync(userId.Value);
        
        if (user is null)
        {
            return Challenge(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidToken,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The specified access token is bound to an account that no longer exists."
                }));
        }
        
        var response = new Dictionary<string, object?>
        {
            ["sub"]   = user.Id,          
            ["name"]  = user.Username,    
            ["email"] = user.Email
        };
        
        if (user.Roles?.Any() == true)
        {
            response["role"] = user.Roles.Select(r => r.Name).ToList();
        }
        
        return Ok(response);
    }
    
    [HttpGet("~/connect/endsession"), HttpPost("~/connect/endsession")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> EndSession()
    {
        var request = HttpContext.GetOpenIddictServerRequest()
                      ?? throw new InvalidOperationException("Missing OIDC request.");
        
        // (1) Kill your local SSO cookie session
        await HttpContext.SignOutAsync("Cookies");
        
        // Hand control back so OpenIddict can validate id_token_hint, redirect, etc.
        return SignOut(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }
}
