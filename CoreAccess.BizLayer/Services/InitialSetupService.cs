using CoreAccess.BizLayer.Helpers;
using CoreAccess.Models;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;

namespace CoreAccess.BizLayer.Services;

public interface IInitialSetupService
{
    bool IsSetupCompleted();
    Task RunSetupAsync(InitialSetupRequest request, CancellationToken cancellationToken = default);
}
public class InitialSetupService(
    ISettingsService settingsService,
    IUserService userService,
    IRoleService roleService,
    IPermissionService permissionRepository,
    IOpenIddictService openIddictService,
    ILogger<InitialSetupService> logger) : IInitialSetupService
{
    private static bool? IsSetupCompletedBuffer { get; set; } 
    private static bool IsBufferInitialized { get; set; }
    
    public bool IsSetupCompleted()
    {   
        if (!IsBufferInitialized)
        {
            var completed = settingsService.GetAsync(SettingsKeys.InitSetupCompleted).Result;
            
            if(completed == null)
                completed = "false";
            
            IsSetupCompletedBuffer = completed == "true";
            
            IsBufferInitialized = true;
        } 
        return IsSetupCompletedBuffer == true;
    }

public async Task RunSetupAsync(InitialSetupRequest request, CancellationToken cancellationToken = default)
{
    logger.LogInformation("Starting initial setup...");

    if (IsSetupCompletedBuffer == true)
    {
        logger.LogWarning("Attempted to run initial setup, but it is already completed.");
        throw new ArgumentException("Initial setup already completed.");
    }

    if (request == null)
    {
        logger.LogError("Initial setup request was null.");
        throw new ArgumentException(nameof(request), "Initial setup request cannot be null.");
    }

    if (string.IsNullOrWhiteSpace(request.GeneralInitialSettings.BaseUri))
    {
        logger.LogError("Initial setup request missing required BaseUri.");
        throw new ArgumentException(nameof(request.GeneralInitialSettings.BaseUri),
            "BaseUri is required for initial setup.");
    }

    logger.LogInformation("Applying general settings...");
    await settingsService.SetAsync(SettingsKeys.BaseUri, request.GeneralInitialSettings.BaseUri, false, cancellationToken);
    await settingsService.SetAsync(SettingsKeys.DisableRegistration, request.GeneralInitialSettings.DisableRegistration, false, cancellationToken);
    await settingsService.SetAsync(SettingsKeys.DatabaseProvider, "Sqlite", false, cancellationToken);

    logger.LogInformation("Applying JWT settings (issuer: {Issuer}, audience: {Audience}, expiresIn: {ExpiresIn})",
        request.JwtInitialSettings.Issuer, request.JwtInitialSettings.Audience, request.JwtInitialSettings.ExpiresIn);

    await settingsService.SetAsync(SettingsKeys.JwtIssuer, request.JwtInitialSettings.Issuer, false, cancellationToken);
    await settingsService.SetAsync(SettingsKeys.JwtAudience, request.JwtInitialSettings.Audience, false, cancellationToken);
    await settingsService.SetAsync(SettingsKeys.JwtExpiresIn, request.JwtInitialSettings.ExpiresIn, false, cancellationToken);

    logger.LogInformation("Registering default OpenIddict applications...");
    await openIddictService.AddApplicationAsync(new OpenIddictApplicationDescriptor()
    {
        ClientId = "coreaccess",
        DisplayName = "CoreAccess AdminUI (and default) Client",
        ApplicationType = OpenIddictConstants.ApplicationTypes.Web,
        ClientType = OpenIddictConstants.ClientTypes.Public,
        ConsentType = OpenIddictConstants.ConsentTypes.Implicit,
        Permissions =
        {
            OpenIddictConstants.Permissions.GrantTypes.Password,
            OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
            OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
            OpenIddictConstants.Permissions.Endpoints.Authorization,
            OpenIddictConstants.Permissions.Endpoints.Introspection,
            OpenIddictConstants.Permissions.Endpoints.EndSession,
            OpenIddictConstants.Permissions.Endpoints.Token,
            OpenIddictConstants.Permissions.ResponseTypes.Token,
            OpenIddictConstants.Permissions.ResponseTypes.Code,
        },
        RedirectUris = { new Uri($"{request.GeneralInitialSettings.BaseUri}/callback") },
        PostLogoutRedirectUris = { new Uri($"{request.GeneralInitialSettings.BaseUri}/") }
    });
    logger.LogInformation("Registered default CoreAccess AdminUI client.");

    await openIddictService.AddApplicationAsync(new OpenIddictApplicationDescriptor()
    {
        ClientId = "postman",
        DisplayName = "Postman Client For API Testing",
        ApplicationType = OpenIddictConstants.ApplicationTypes.Web,
        ClientType = OpenIddictConstants.ClientTypes.Public,
        ConsentType = OpenIddictConstants.ConsentTypes.Implicit,
        Permissions =
        {
            OpenIddictConstants.Permissions.GrantTypes.Password,
            OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
            OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
            OpenIddictConstants.Permissions.Endpoints.Authorization,
            OpenIddictConstants.Permissions.Endpoints.Introspection,
            OpenIddictConstants.Permissions.Endpoints.EndSession,
            OpenIddictConstants.Permissions.Endpoints.Token,
            OpenIddictConstants.Permissions.ResponseTypes.Token,
            OpenIddictConstants.Permissions.ResponseTypes.Code,
        },
        RedirectUris = { new Uri("http://localhost:5000/callback") },
        PostLogoutRedirectUris = { new Uri("http://localhost:5000/") }
    });
    logger.LogInformation("Registered Postman client for API testing.");

    logger.LogInformation("Creating default roles...");
    var adminRole = await roleService.CreateRoleAsync(new RoleCreateRequest()
    {
        Name = "CoreAccess.Admin",
        Description = "CoreAccess Admin role for administrative access to CoreAccess"
    }, cancellationToken);
    logger.LogInformation("Created role {RoleName} with id {RoleId}.", adminRole.Name, adminRole.Id);

    var userRole = await roleService.CreateRoleAsync(new RoleCreateRequest()
    {
        Name = "User",
        Description = "User role for default users"
    }, cancellationToken);
    logger.LogInformation("Created role {RoleName} with id {RoleId}.", userRole.Name, userRole.Id);

    logger.LogInformation("Creating default system permissions...");
    List<PermissionCreateRequest> permissions = new()
    {
        new() { Name = "user.read", Description = "Read users", IsSystem = true },
        new() { Name = "user.write", Description = "Create/update/delete users", IsSystem = true },
        new() { Name = "role.read", Description = "Read roles", IsSystem = true },
        new() { Name = "role.write", Description = "Create/update/delete roles", IsSystem = true },
        new() { Name = "permission.read", Description = "Read permissions", IsSystem = true },
        new() { Name = "permission.write", Description = "Manage permissions", IsSystem = true },
        new() { Name = "settings.read", Description = "Read system settings", IsSystem = true },
        new() { Name = "settings.write", Description = "Update system settings", IsSystem = true }
    };

    foreach (var permission in permissions)
    {
        var createdPermission = await permissionRepository.CreatePermissionAsync(permission, cancellationToken: cancellationToken);
        if (createdPermission == null)
        {
            logger.LogError("Failed to create permission {Permission}.", permission.Name);
            throw new Exception($"Failed to create permission: {permission.Name}");
        }
        await roleService.AddPermissionToRoleAsync(adminRole.Id.ToString(), createdPermission.Id.ToString(), cancellationToken);
        logger.LogInformation("Created permission {Permission} and assigned to role {Role}.", createdPermission.Name, adminRole.Name);
    }

    if (request.UserInitialSettings.Admin == null)
    {
        logger.LogInformation("No admin user provided in setup request. Generating default root user...");
        string password = SecureKeyHelper.GenerateSecurePassword();
        request.UserInitialSettings.Admin = new UserCreateRequest
        {
            Username = "root",
            Email = "root@coreaccess.com",
            Password = password,
        };
    }

    var newUser = await userService.CreateUserAsync(request.UserInitialSettings.Admin, cancellationToken: cancellationToken);
    logger.LogInformation("Created admin user {Username} with id {UserId}.", newUser.Username, newUser.Id);

    await userService.AddRoleToUserAsync(newUser.Id.ToString(), adminRole.Id.ToString(), cancellationToken: cancellationToken);
    logger.LogInformation("Assigned role {Role} to admin user {Username}.", adminRole.Name, newUser.Username);
    
    logger.LogDebug("Adding Encrypted Test Database setting for testing purposes.");
    await settingsService.SetAsync("Debug:TestEncryptedSetting", "TestEncryptedValue", true, cancellationToken);

    await SaveCompletedAsync();
    logger.LogInformation("Initial setup completed successfully.");
}


    private async Task SaveCompletedAsync()
    {
        await settingsService.SetAsync(SettingsKeys.InitSetupCompleted, "true", false);
        IsSetupCompletedBuffer = true;
    }
}
