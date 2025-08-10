using System.Security.Claims;
using CoreAccess.Models;
using OpenIddict.Abstractions;

namespace CoreAccess.BizLayer.Services;

public interface IOpenIddictService
{
    List<Claim> GetUserClaims(UserDto user);
    public Task AddApplicationAsync(OpenIddictApplicationDescriptor application);
}

public class OpenIddictService(IOpenIddictApplicationManager applicationManager) : IOpenIddictService
{
    public List<Claim> GetUserClaims(UserDto user)
    {
        List<Claim> claims = new List<Claim>();
        
        claims.Add(new(OpenIddictConstants.Claims.Subject, user.Id.ToString()));
        claims.Add(new(OpenIddictConstants.Claims.Name, user.Username));
        
        if(!string.IsNullOrWhiteSpace(user.Email))
            claims.Add(new Claim(OpenIddictConstants.Claims.Email, user.Email));
        
        foreach (var role in user.Roles)
        {
            claims.Add(new Claim(OpenIddictConstants.Claims.Role, role.Name));
        }

        return claims;
    }
    
    public async Task AddApplicationAsync(OpenIddictApplicationDescriptor application)
    {
        if (application == null) throw new ArgumentNullException(nameof(application));
        
        var app = await applicationManager.FindByClientIdAsync(application.ClientId);
        if (app != null) return;

        await applicationManager.CreateAsync(application);
    }
    
}