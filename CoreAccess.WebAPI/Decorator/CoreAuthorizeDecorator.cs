using CoreAccess.WebAPI.Model;
using CoreAccess.WebAPI.Services;
using CoreAccess.WebAPI.Services.CoreAuth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

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
        
        var tokenService = httpContext.RequestServices.GetService<ICoreAccessTokenService>();

        var claimsPrincipal = tokenService.ValidateToken(token);
        
        if (claimsPrincipal == null)
        {
            context.Result = new UnauthorizedResult();
            return Task.CompletedTask;
        }

        var userId = tokenService.GetClaim(claimsPrincipal, "coreaccess:user_id");
        
        if (string.IsNullOrWhiteSpace(userId.Value))
        {
            context.Result = new UnauthorizedResult();
            return Task.CompletedTask;
        }
        
        if (!string.IsNullOrWhiteSpace(Roles))
        {
            var roles = Roles.Split(',').Select(r => r.Trim()).ToList();
            var userRoles = tokenService.GetClaim(claimsPrincipal, "coreaccess:roles").Value.Split(',')
                .Select(r => r.Trim()).ToList();

            if (!roles.Any(role => userRoles.Contains(role)))
            {
                context.Result = new ForbidResult();
                return Task.CompletedTask;
            }
        }
        
        if (!string.IsNullOrWhiteSpace(Permissions))
        {
            var permissions = Permissions.Split(',').Select(p => p.Trim()).ToList();
            var userPermissions = tokenService.GetClaim(claimsPrincipal, "coreaccess:permissions").Value.Split(',')
                .Select(p => p.Trim()).ToList();

            if (!permissions.Any(permission => userPermissions.Contains(permission)))
            {
                context.Result = new ForbidResult();
                return Task.CompletedTask;
            }
        }

        return Task.CompletedTask;
    }
}
