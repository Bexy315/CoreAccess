namespace CoreAccess.WebAPI.Services;

public interface ICoreAccessTokenService
{
    // Define methods for managing access tokens, e.g., generating, validating, revoking tokens
}

public class CoreAccessTokenService(AppSettingsService appSettingsService) : ICoreAccessTokenService
{
    
}