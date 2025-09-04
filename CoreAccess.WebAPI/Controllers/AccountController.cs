using System.Security.Claims;
using CoreAccess.BizLayer.Services;
using CoreAccess.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;

namespace CoreAccess.WebAPI.Controllers;

[Route("account")]
public class AccountController(IUserService userService) : Controller
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
        var user = await userService.SearchUsersAsync(new UserSearchOptions()
        {
            Username = username,
            PageSize = 1
        }).ContinueWith(task => task.Result.Items.FirstOrDefault());
        
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
        
        var claims = new List<Claim>
        {
            new Claim(OpenIddictConstants.Claims.Subject, user.Id.ToString()),
            new Claim(OpenIddictConstants.Claims.Name, user.Username)
        };

        var identity = new ClaimsIdentity(claims, "Cookies");
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync("Cookies", principal);

        return Redirect(returnUrl ?? "/");
    }
}