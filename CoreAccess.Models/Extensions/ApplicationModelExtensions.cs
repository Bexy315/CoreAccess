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
            ClientType = entity.ClientType
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
            RedirectUris = entity.RedirectUris?.Replace("\"", "").Replace("[", "").Replace("]", "").Split(",").ToList(),
            PostLogoutRedirectUris = entity.PostLogoutRedirectUris?.Replace("\"", "").Replace("[", "").Replace("]", "").Split(",").ToList(),
            Permissions = entity.Permissions?.Replace("\"", "").Replace("[", "").Replace("]", "").Split(",").ToList(),
            Requirements = entity.Requirements,
        };
}