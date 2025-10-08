using System.Security.Claims;
using CoreAccess.BizLayer.Services;
using CoreAccess.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;

namespace CoreAccess.WebAPI.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
[Route("account")]
public class AccountController(IUserService userService, ISettingsService settingsService, IOpenIddictService openIddictService) : Controller
{
    [HttpGet("login")]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
    {
        var user = await userService.GetUserByUsernameAsync(username);
        
        if (user == null)
        {
            ModelState.AddModelError("", "User not found");
            return View();
        }
        
        if (!await userService.ValidateCredentialsByUsernameAsync(user.Username, password))
        {
            ModelState.AddModelError("", "Invalid credentials");
            return View();
        }
        
        var claims = await openIddictService.GetUserClaims(user);

        var identity = new ClaimsIdentity(claims, "Cookies");
        var principal = new ClaimsPrincipal(identity);
        
        var tokenLifetime = await settingsService.GetTokenLifetimeAsync();
        principal.SetAccessTokenLifetime(TimeSpan.FromSeconds(tokenLifetime));

        await HttpContext.SignInAsync("Cookies", principal);

        return Redirect(returnUrl ?? "/");
    }
}