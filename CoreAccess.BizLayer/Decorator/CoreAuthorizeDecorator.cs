using CoreAccess.BizLayer.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace CoreAccess.WebAPI.Decorator;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class CoreAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
{
    public string? Roles { get; set; }
    public string? Permissions { get; set; }

    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var httpContext = context.HttpContext;
        
        var token = httpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(' ').Last();
        
        if (string.IsNullOrWhiteSpace(token))
        {
            context.Result = new UnauthorizedResult();
            return Task.CompletedTask;
        }
        
        var tokenService = httpContext.RequestServices.GetService<ITokenService>();

        var claimsPrincipal = tokenService.ValidateTokenAsync(token).Result;
        
        if (claimsPrincipal == null)
        {
            context.Result = new UnauthorizedResult();
            return Task.CompletedTask;
        }

        var userId = tokenService.GetClaim(claimsPrincipal, "sub");
        
        if (string.IsNullOrWhiteSpace(userId.Value))
        {
            context.Result = new UnauthorizedResult();
            return Task.CompletedTask;
        }
        
        if (!string.IsNullOrWhiteSpace(Roles))
        {
            var roles = Roles.Split(',').Select(r => r.Trim()).ToList();
            var userRoles = claimsPrincipal.Claims
                .Where(c => c.Type == "role")
                .Select(c => c.Value)
                .ToList();
            
            if (!roles.Any(role => userRoles.Contains(role)))
            {
                context.Result = new ObjectResult("Forbidden") { StatusCode = StatusCodes.Status403Forbidden };
                return Task.CompletedTask;
            }
        }

        return Task.CompletedTask;
    }
}
