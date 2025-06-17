namespace CoreAccess.WebAPI.Model;

public class InitialSetupRequest 
{
    public CoreUserCreateRequest Admin { get; set; } = new();
    public string JwtSecret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string ExpiresIn { get; set; } = "60"; // Default to 1 hour
}