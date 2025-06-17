using CoreAccess.WebAPI.Model;

namespace CoreAccess.WebAPI.Services.InitialSetup;

public class InitialSetupService(
    SetupStatusService status,
    ILogger<InitialSetupService> logger,
    IAppSettingsService appSettingsService,
    IUserService userService)
{
    public async Task RunSetupAsync(InitialSetupRequest request)
    {
        status.StartSetup();
        if (request == null)
        {
            logger.LogError("Initial setup request cannot be null.");
            throw new ArgumentNullException(nameof(request), "Initial setup request cannot be null.");
        }
        
        status.UpdateSetupProgress("Creating Admin User", 33);
        
        await userService.CreateUserAsync(request.Admin);
        
        status.UpdateSetupProgress("Configure JWT Settings", 66);
        
        if (string.IsNullOrWhiteSpace(request.JwtSecret) || string.IsNullOrWhiteSpace(request.Issuer) ||
            string.IsNullOrWhiteSpace(request.Audience) || string.IsNullOrWhiteSpace(request.ExpiresIn))
        {
            logger.LogError("JWT configuration is incomplete. Please provide all required fields.");
            throw new ArgumentException("JWT configuration is incomplete. Please provide all required fields.");
        }
        
        appSettingsService.Set(AppSettingsKeys.JwtSecretKey, request.JwtSecret);
        appSettingsService.Set(AppSettingsKeys.JwtIssuer, request.Issuer);
        appSettingsService.Set(AppSettingsKeys.JwtAudience, request.Audience);
        appSettingsService.Set(AppSettingsKeys.JwtExpiresIn, request.ExpiresIn);
        
        status.UpdateSetupProgress("Finalize Setup", 99);

        status.CompleteSetup();
    }
}
