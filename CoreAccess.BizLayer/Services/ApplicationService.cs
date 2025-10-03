using CoreAccess.Models;
using CoreAccess.Models.Extensions;
using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;

namespace CoreAccess.BizLayer.Services;

public interface IApplicationService
{
    public Task<PagedResult<ApplicationDto>> GetApplications(ApplicationSearchOptions options, CancellationToken cancellationToken = default);
    public Task<ApplicationDetailDto> GetApplication(string applicationId, CancellationToken cancellationToken = default);
    public Task AddApplicationAsync(OpenIddictApplicationDescriptor application);
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
                if (index >= skip && result.Items.Count < take)
                {
                    result.Items.Add(appEntity.ToDto());
                }
                index++;
                if (result.Items.Count >= take)
                    break;
            }
        }
        
        result.TotalCount = result.Items.Count;
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
    
    public async Task AddApplicationAsync(OpenIddictApplicationDescriptor application)
    {
        if (application == null) throw new ArgumentNullException(nameof(application));
        
        var app = await applicationManager.FindByClientIdAsync(application.ClientId);
        if (app != null) return;

        await applicationManager.CreateAsync(application);
    }
}