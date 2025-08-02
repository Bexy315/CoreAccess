using Microsoft.AspNetCore.Mvc;
using CoreAccess.WebAPI.Model;
using CoreAccess.WebAPI.Services;

namespace CoreAccess.WebAPI.Controllers;

[Controller]
[Route("api/auth")]
public class AuthController(IAppSettingsService appSettingsService, IUserService userService, ITokenService tokenService) : ControllerBase
{
    
    [HttpPost("login")]
    [Produces(typeof(LoginResponse))]
    public async Task<IActionResult> Login([FromBody] LoginRequest dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await userService.ValidateCredentialsByUsernameAsync(dto.Username, dto.Password,
                cancellationToken);

            var accessToken = tokenService.GenerateAccessToken(user, cancellationToken: cancellationToken);
            var refreshToken = await tokenService.GenerateRefreshTokenAsync(user, cancellationToken);

            return Ok(new LoginResponse()
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                UserId = user.Id
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
    
    [HttpPost("refresh-token")]
    public async Task<IActionResult> Refresh([FromBody] string refreshToken, CancellationToken cancellationToken = default)
    {
        try
        {
           return Ok(await tokenService.RefreshTokenAsync(refreshToken, cancellationToken));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
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
    
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] string refreshToken, CancellationToken cancellationToken = default)
    {
        try
        {
            await tokenService.RevokeRefreshTokenAsync(refreshToken, 
                cancellationToken: cancellationToken);
            return Ok();
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
