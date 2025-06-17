using CoreAccess.WebAPI.Helpers;
using CoreAccess.WebAPI.Logger;
using CoreAccess.WebAPI.Logger.Model;
using CoreAccess.WebAPI.Model;

namespace CoreAccess.WebAPI.Services;

public class InitialSetupService(
    ILogger<InitialSetupService> logger,
    IAppSettingsService appSettingsService,
    IUserService userService)
{
    public async Task RunSetupAsync(InitialSetupRequest request)
    {
        if (request == null)
        {
            logger.LogError("Initial setup request cannot be null.");
            throw new ArgumentNullException(nameof(request), "Initial setup request cannot be null.");
        }

        if (request.Admin == null)
        {
            string password = SecureKeyHelper.GenerateSecurePassword();
            request.Admin = new CoreUserCreateRequest
            {
                Username = "root",
                Email = "root@coreaccess.com",
                Password = password
            };
            CoreLogger.LogSystem(CoreLogLevel.Warning, nameof(InitialSetupService), "Admin user not provided, using default root user with a generated password. Please change it after setup. Password: "+ password);
        }
        
        var newUser = await userService.CreateUserAsync(request.Admin);
        await userService.AddRoleToUserAsync(newUser.Id.ToString(), "CoreAccess.Admin");
        
        CoreLogger.LogSystem(CoreLogLevel.Information, nameof(InitialSetupService), "User '" + request.Admin.Username + "' created successfully.");
        
        if (string.IsNullOrWhiteSpace(request.JwtSecret))
            request.JwtSecret = SecureKeyHelper.GenerateRandomBase64Key();
        
        appSettingsService.Set(AppSettingsKeys.JwtSecretKey, request.JwtSecret, true, true);
        appSettingsService.Set(AppSettingsKeys.JwtIssuer, request.Issuer, isSystem: true);
        appSettingsService.Set(AppSettingsKeys.JwtAudience, request.Audience, isSystem: true);
        appSettingsService.Set(AppSettingsKeys.JwtExpiresIn, request.ExpiresIn, isSystem: true);
        appSettingsService.Set(AppSettingsKeys.SystemLogLevel, request.SystemLogLevel, isSystem: true);
        appSettingsService.Set(AppSettingsKeys.DisableRegistration, request.DisableRegistration, isSystem: true);
        
        CoreLogger.LogSystem(CoreLogLevel.Information, nameof(InitialSetupService), "JWT settings configured successfully.");

        await SaveCompleted();
        
        appSettingsService.Reload();
                
        CoreLogger.LogSystem(CoreLogLevel.Information, nameof(InitialSetupService), "Initial setup completed successfully.");
    }
    
    private async Task SaveCompleted()
    {
        var filePath = "/var/data/etc/init_setup_completed.txt";

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
}
