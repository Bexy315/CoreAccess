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
[Route("api")]
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
                claim.SetDestinations(OpenIddictConstants.Destinations.AccessToken);
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

        return BadRequest("Unsupported grant type.");
    }
    
    [HttpPost("register")]
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
