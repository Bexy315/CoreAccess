using CoreAccess.WebAPI.Helpers;
using CoreAccess.WebAPI.Logger;
using CoreAccess.WebAPI.Logger.Model;
using CoreAccess.WebAPI.Model;
using CoreAccess.WebAPI.Repositories;

namespace CoreAccess.WebAPI.Services;

public class InitialSetupService(
    IAppSettingsService appSettingsService,
    IUserService userService,
    IRoleService roleService,
    IPermissionRepository permissionRepository)
{
    public async Task RunSetupAsync(InitialSetupRequest request)
    {
        if(await IsSetupCompletedAsync())
        {
            CoreLogger.LogSystem(CoreLogLevel.Error, nameof(InitialSetupService),"Initial setup already completed.");
            throw new ArgumentException("Initial setup already completed.");
        }
        
        if (request == null)
        {
            CoreLogger.LogSystem(CoreLogLevel.Error, nameof(InitialSetupService),"Initial setup request cannot be null.");
            throw new ArgumentException(nameof(request), "Initial setup request cannot be null.");
        }
        
        if (string.IsNullOrWhiteSpace(request.GeneralInitialSettings.BaseUri))
        {
            CoreLogger.LogSystem(CoreLogLevel.Error, nameof(InitialSetupService),"BaseUri is required for initial setup.");
            throw new ArgumentException(nameof(request.GeneralInitialSettings.BaseUri), "BaseUri is required for initial setup.");
        }
        
        appSettingsService.Set(AppSettingsKeys.BaseUri, request.GeneralInitialSettings.BaseUri, isSystem: true);
        appSettingsService.Set(AppSettingsKeys.SystemLogLevel, request.GeneralInitialSettings.SystemLogLevel, isSystem: true);
        appSettingsService.Set(AppSettingsKeys.DisableRegistration, request.GeneralInitialSettings.DisableRegistration, isSystem: true);
        
        CoreLogger.LogSystem(CoreLogLevel.Information, nameof(InitialSetupService), "General settings configured successfully.");
        
        if (string.IsNullOrWhiteSpace(request.JwtInitialSettings.JwtSecret))
            request.JwtInitialSettings.JwtSecret = SecureKeyHelper.GenerateRandomBase64Key();
        
        appSettingsService.Set(AppSettingsKeys.JwtSecretKey, request.JwtInitialSettings.JwtSecret, true, true);
        appSettingsService.Set(AppSettingsKeys.JwtIssuer, request.JwtInitialSettings.Issuer, isSystem: true);
        appSettingsService.Set(AppSettingsKeys.JwtAudience, request.JwtInitialSettings.Audience, isSystem: true);
        appSettingsService.Set(AppSettingsKeys.JwtExpiresIn, request.JwtInitialSettings.ExpiresIn, isSystem: true);
        
        CoreLogger.LogSystem(CoreLogLevel.Information, nameof(InitialSetupService), "JWT settings configured successfully.");
        
        
        var adminRole = await roleService.CreateRoleAsync(new RoleCreateRequest()
        {
            Name = "CoreAccess.Admin",
            Description = "CoreAccess Admin role for administrative access to CoreAccess"
        });
        
        await roleService.CreateRoleAsync(new RoleCreateRequest()
        {
            Name = "User",
            Description = "User role for default users"
        });
        
        List<CreatePermissionRequest> permissions = new()
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
            var createdPermission = await permissionRepository.AddPermissionAsync(permission);
            if (createdPermission == null)
            {
                CoreLogger.LogSystem(CoreLogLevel.Error, nameof(InitialSetupService),$"Failed to create permission: {permission.Name}");
                throw new Exception($"Failed to create permission: {permission.Name}");
            }
            await roleService.AddPermissionToRoleAsync(adminRole.Name, createdPermission.Name);
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
            CoreLogger.LogSystem(CoreLogLevel.Warning, nameof(InitialSetupService), "Admin user not provided, using default root user with a generated password. Please change it after setup. Password: "+ password);
        }
        
        var newUser = await userService.CreateUserAsync(request.UserInitialSettings.Admin);
        await userService.AddRoleToUserAsync(newUser.Id.ToString(), "CoreAccess.Admin");
        
        CoreLogger.LogSystem(CoreLogLevel.Information, nameof(InitialSetupService), "User '" + request.UserInitialSettings.Admin.Username + "' created successfully.");
        

        await SaveCompletedAsync();
        
        appSettingsService.Reload();
                
        CoreLogger.LogSystem(CoreLogLevel.Information, nameof(InitialSetupService), "Initial setup completed successfully.");
    }
    
    private async Task SaveCompletedAsync()
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, "/data/etc/init_setup_completed.txt");
        
        if (File.Exists(filePath))
        {
            await File.WriteAllTextAsync(filePath, "true");
            return;
        }

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(filePath, "true");
    }
    
    public async Task <bool> IsSetupCompletedAsync(CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, "/data/etc/init_setup_completed.txt");
        return (await File.ReadAllTextAsync(filePath, cancellationToken)).Trim().Equals("true", StringComparison.OrdinalIgnoreCase);
    }
    
    private static string Now() => DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
}
