using OpenIddict.EntityFrameworkCore.Models;

namespace CoreAccess.Models.Extensions;

public static class ApplicationModelExtensions
{
    public static ApplicationDto ToDto(this OpenIddictEntityFrameworkCoreApplication entity) =>
        new ApplicationDto
        {
            Id = entity.Id.ToString(),
            ClientId = entity.ClientId,
            DisplayName = entity.DisplayName,
            ClientType = entity.ClientType,
            RedirectUris = entity.RedirectUris,
            PostLogoutRedirectUris = entity.PostLogoutRedirectUris,
        };
    
    public static ApplicationDetailDto ToDetailDto(this OpenIddictEntityFrameworkCoreApplication entity) =>
        new ApplicationDetailDto
        {
            Id = entity.Id,
            ClientId = entity.ClientId,
            DisplayName = entity.DisplayName,
            ApplicationType = entity.ApplicationType,
            ClientType = entity.ClientType,
            ConsentType = entity.ConsentType,
            ClientSecret = entity.ClientSecret,
            RedirectUris = entity.RedirectUris,
            PostLogoutRedirectUris = entity.PostLogoutRedirectUris,
            Permissions = entity.Permissions,
            Requirements = entity.Requirements,
        };
}