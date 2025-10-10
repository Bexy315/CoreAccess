using CoreAccess.Models;
using CoreAccess.Models.Extensions;
using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;

namespace CoreAccess.BizLayer.Services;

public interface IApplicationService
{
    public Task<PagedResult<ApplicationDto>> GetApplications(ApplicationSearchOptions options, CancellationToken cancellationToken = default);
    public Task<ApplicationDetailDto> GetApplication(string applicationId, CancellationToken cancellationToken = default);
    public Task AddApplicationAsync(OpenIddictApplicationDescriptor application, CancellationToken cancellationToken = default);
    public Task UpdateApplicationAsync(string id, ApplicationUpdateRequest request, CancellationToken cancellationToken = default);
}

public class ApplicationService(IOpenIddictApplicationManager applicationManager) : IApplicationService
{
    public async Task<PagedResult<ApplicationDto>> GetApplications(ApplicationSearchOptions options, CancellationToken cancellationToken = default)
    {
        var applications = applicationManager.ListAsync(cancellationToken: cancellationToken);

        var result = new PagedResult<ApplicationDto>()
        {
            Page = options.Page,
            PageSize = options.PageSize,
            Items = new List<ApplicationDto>()
        };

        int skip = (options.Page - 1) * options.PageSize;
        int take = options.PageSize;
        int index = 0;

        await foreach (var application in applications)
        {
            var appEntity = application as OpenIddictEntityFrameworkCoreApplication;

            if (appEntity == null)
                continue;

            if (string.IsNullOrWhiteSpace(options.Search) ||
                (!string.IsNullOrWhiteSpace(options.Search) &&
                 (appEntity.ClientId.Contains(options.Search, StringComparison.OrdinalIgnoreCase) ||
                  appEntity.DisplayName.Contains(options.Search, StringComparison.OrdinalIgnoreCase))))
            {
                if (index >= skip && result.Items.Count() < take)
                {
                    ((List<ApplicationDto>)result.Items).Add(appEntity.ToDto());
                }
                index++;
                if (result.Items.Count() >= take)
                    break;
            }
        }

        result.TotalCount = index;
        return result;
    }

    public async Task<ApplicationDetailDto> GetApplication(string applicationId, CancellationToken cancellationToken = default)
    {
        var application = await applicationManager.FindByIdAsync(applicationId, cancellationToken);
        if (application == null)
            return null;

        var appEntity = application as OpenIddictEntityFrameworkCoreApplication;
        if (appEntity == null)
            return null;

        return appEntity.ToDetailDto();
    }

    public async Task AddApplicationAsync(OpenIddictApplicationDescriptor application, CancellationToken cancellationToken = default)
    {
        if (application == null) throw new ArgumentNullException(nameof(application));

        var app = await applicationManager.FindByClientIdAsync(application.ClientId, cancellationToken);
        if (app != null) return;

        await applicationManager.CreateAsync(application, cancellationToken);
    }

    public async Task UpdateApplicationAsync(string id, ApplicationUpdateRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        var appObj = await applicationManager.FindByIdAsync(id, cancellationToken);
        
        if (appObj == null)
            throw new ArgumentException($"Application with ID {id} not found.");
        
        var app = appObj as OpenIddictEntityFrameworkCoreApplication;
        
        if (app == null)
            throw new ArgumentException("Invalid request object.");

        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = string.IsNullOrWhiteSpace(request.ClientId) ? app.ClientId : request.ClientId,
            DisplayName = string.IsNullOrWhiteSpace(request.DisplayName) ? app.DisplayName : request.DisplayName,
            ClientType = app.ClientType,
            ConsentType = app.ConsentType,
            ApplicationType = app.ApplicationType,
            ClientSecret = string.IsNullOrWhiteSpace(request.ClientSecret) ? app.ClientSecret : request.ClientSecret
        };

        if (request.RedirectUris != null){
            foreach (var uri in request.RedirectUris)
                descriptor.RedirectUris.Add(new Uri(uri));
        }
        else if (app.RedirectUris != null)
        {
            foreach (var uri in app.RedirectUris?.Replace("\"", "").Replace("[", "").Replace("]", "").Split(",").ToList())
                descriptor.RedirectUris.Add(new Uri(uri));
        }
        
        if (request.PostLogoutRedirectUris != null){
            foreach (var uri in request.PostLogoutRedirectUris)
                descriptor.PostLogoutRedirectUris.Add(new Uri(uri));
        }
        else if (app.PostLogoutRedirectUris != null)
        {
            foreach (var uri in app.PostLogoutRedirectUris?.Replace("\"", "").Replace("[", "").Replace("]", "").Split(",").ToList())
                descriptor.PostLogoutRedirectUris.Add(new Uri(uri));
        }

        if (request.Permissions != null)
        {
            foreach (var permission in request.Permissions)
                descriptor.Permissions.Add(permission);
        }
        else if (app.Permissions != null)
        {
            foreach (var permission in app.Permissions?.Replace("\"", "").Replace("[", "").Replace("]", "").Split(",").ToList())
                descriptor.Permissions.Add(permission);
        }

        if (request.Requirements != null)
        {
            foreach (var requirement in request.Requirements)
                descriptor.Requirements.Add(requirement);
        }
        else if (app.Requirements != null)
        {
            foreach (var requirement in app.Requirements?.Replace("\"", "").Replace("[", "").Replace("]", "").Split(",").ToList())
                descriptor.Requirements.Add(requirement);
        }

        await applicationManager.UpdateAsync(appObj, descriptor, cancellationToken);
    }
}
