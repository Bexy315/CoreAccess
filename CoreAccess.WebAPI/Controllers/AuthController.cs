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
public class AuthController(IAppSettingsService appSettingsService, IUserService userService, IOpenIddictService openIddictService, ITokenService tokenService) : ControllerBase
{
    [HttpPost("connect/token")]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest();
        if (request == null)
            throw new InvalidOperationException("OpenIddict request is null.");
        
        if (request.IsPasswordGrantType())
        {
            var user = await userService.SearchUsersAsync(new UserSearchOptions()
            {
                Username = request.Username,
                PageSize = 1
            });
            
            if (user == null || user.TotalCount == 0 || !await userService.ValidateCredentialsByUsernameAsync(user.Items.FirstOrDefault().Username, request.Password))
            {
                return Forbid();
            }
            var claims = openIddictService.GetUserClaims(user.Items.FirstOrDefault());
            
            foreach (var claim in claims)
            {
                claim.SetDestinations(OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken, OpenIddictConstants.Destinations.IssuedToken);
            }

            var identity = new ClaimsIdentity(claims, TokenValidationParameters.DefaultAuthenticationType);
            var principal = new ClaimsPrincipal(identity);

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
            var userRes = await userService.SearchUsersAsync(new UserSearchOptions { Id = userId, PageSize = 1 });
            var user = userRes.Items.FirstOrDefault();
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
            
            var freshClaims = openIddictService.GetUserClaims(user);
            foreach (var c in freshClaims)
                c.SetDestinations(OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken);

            var freshIdentity = new ClaimsIdentity(TokenValidationParameters.DefaultAuthenticationType);
            freshIdentity.AddClaims(freshClaims);
            var freshPrincipal = new ClaimsPrincipal(freshIdentity);

            freshPrincipal.SetScopes(result.Principal!.GetScopes());
            freshPrincipal.SetResources("coreaccess-api");

            return SignIn(freshPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }


        return BadRequest("Unsupported grant type.");
    }
    
    [HttpGet("connect/authorize")]
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
        
        var user = await userService.SearchUsersAsync(new UserSearchOptions()
        {
            Id = userId,
            PageSize = 1
        });
        
        var claims = openIddictService.GetUserClaims(user.Items.FirstOrDefault());
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
        var userResult = await userService.SearchUsersAsync(new UserSearchOptions()
        {
            Id = User.FindFirstValue(OpenIddictConstants.Claims.Subject),
            PageSize = 1
        });
        
        var user = userResult.Items.FirstOrDefault();
        
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
    
    [HttpPost("api/register")]
    [Produces(typeof(UserDto))]
    public async Task<IActionResult> Register([FromBody] RegisterRequest dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (appSettingsService.Get(AppSettingsKeys.DisableRegistration) == "true")
                return StatusCode(403, "Registration is disabled.");
            
            if (await userService.UsernameExistsAsync(dto.Username, cancellationToken))
                return BadRequest("Username already exists.");

            var user = await userService.CreateUserAsync(new UserCreateRequest(){Password = dto.Password, Username = dto.Username}, cancellationToken);
            return Ok(user);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, ex.Message);
        }
    }
}
