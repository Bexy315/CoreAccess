using CoreAccess.Models;
using CoreAccess.WebAPI.Helpers;
using OpenIddict.Abstractions;

namespace CoreAccess.BizLayer.Services;

public interface IInitialSetupService
{
    bool IsSetupCompleted();
    Task RunSetupAsync(InitialSetupRequest request, CancellationToken cancellationToken = default);
}
public class InitialSetupService(
    IAppSettingsService appSettingsService,
    IUserService userService,
    IRoleService roleService,
    IPermissionService permissionRepository,
    IOpenIddictService openIddictService) : IInitialSetupService
{
    private static bool? IsSetupCompletedBuffer { get; set; } 
    private static bool IsBufferInitialized { get; set; }
    
    public bool IsSetupCompleted()
    {   
        if (!IsBufferInitialized)
        {
          //var filePath = Path.Combine(AppContext.BaseDirectory, "data", "etc", "init_setup_completed.txt"); 
            var filePath = "E:\\data\\etc\\init_setup_completed.txt";
            IsSetupCompletedBuffer = File.Exists(filePath) && File.ReadAllText(filePath).Trim().Equals("true", StringComparison.OrdinalIgnoreCase);
            IsBufferInitialized = true;
        } 
        return IsSetupCompletedBuffer == true;
    }

    public async Task RunSetupAsync(InitialSetupRequest request, CancellationToken cancellationToken = default)
    {
        if (IsSetupCompletedBuffer == true)
        {
            throw new ArgumentException("Initial setup already completed.");
        }

        if (request == null)
        {
            throw new ArgumentException(nameof(request), "Initial setup request cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(request.GeneralInitialSettings.BaseUri))
        {
            throw new ArgumentException(nameof(request.GeneralInitialSettings.BaseUri),
                "BaseUri is required for initial setup.");
        }

        appSettingsService.Set(AppSettingsKeys.BaseUri, request.GeneralInitialSettings.BaseUri, isSystem: true);
        appSettingsService.Set(AppSettingsKeys.SystemLogLevel, request.GeneralInitialSettings.SystemLogLevel,
            isSystem: true);
        appSettingsService.Set(AppSettingsKeys.DisableRegistration, request.GeneralInitialSettings.DisableRegistration,
            isSystem: true);

        if (string.IsNullOrWhiteSpace(request.JwtInitialSettings.JwtSecret))
            request.JwtInitialSettings.JwtSecret = SecureKeyHelper.GenerateRandomBase64Key();

        appSettingsService.Set(AppSettingsKeys.JwtSecretKey, request.JwtInitialSettings.JwtSecret, true, true);
        appSettingsService.Set(AppSettingsKeys.JwtIssuer, request.JwtInitialSettings.Issuer, isSystem: true);
        appSettingsService.Set(AppSettingsKeys.JwtAudience, request.JwtInitialSettings.Audience, isSystem: true);
        appSettingsService.Set(AppSettingsKeys.JwtExpiresIn, request.JwtInitialSettings.ExpiresIn, isSystem: true);

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
            RedirectUris =
            {
                new Uri("http://localhost:5173/callback")
            },
            PostLogoutRedirectUris =
            {
                new Uri("http://localhost:5173/")
            }
        });
        
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
            RedirectUris =
            {
                new Uri("http://localhost:5173/callback")
            },
            PostLogoutRedirectUris =
            {
                new Uri("http://localhost:5173/")
            }
        });
        
        
        var adminRole = await roleService.CreateRoleAsync(new RoleCreateRequest()
        {
            Name = "CoreAccess.Admin",
            Description = "CoreAccess Admin role for administrative access to CoreAccess"
        }, cancellationToken);
        
        await roleService.CreateRoleAsync(new RoleCreateRequest()
        {
            Name = "User",
            Description = "User role for default users"
        }, cancellationToken);
        
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
                throw new Exception($"Failed to create permission: {permission.Name}");
            }
            await roleService.AddPermissionToRoleAsync(adminRole.Id.ToString(), createdPermission.Id.ToString(), cancellationToken);
        }

        if (request.UserInitialSettings.Admin == null)
        {
            string password = SecureKeyHelper.GenerateSecurePassword();
            request.UserInitialSettings.Admin = new UserCreateRequest
            {
                Username = "root",
                Email = "root@coreaccess.com",
                Password = password,
            };
        }
        
        var newUser = await userService.CreateUserAsync(request.UserInitialSettings.Admin, cancellationToken: cancellationToken);
        await userService.AddRoleToUserAsync(newUser.Id.ToString(), adminRole.Id.ToString(), cancellationToken: cancellationToken);

        await SaveCompletedAsync();
        
        appSettingsService.Reload();
    }

    private async Task SaveCompletedAsync()
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, "/data/etc/init_setup_completed.txt");
        
        if (File.Exists(filePath))
        {
            await File.WriteAllTextAsync(filePath, "true");
            IsSetupCompletedBuffer = true;
            return;
        }

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(filePath, "true");
        IsSetupCompletedBuffer = true;
    }
}
