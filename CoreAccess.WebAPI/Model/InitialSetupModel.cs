namespace CoreAccess.WebAPI.Model;

public class InitialSetupRequest 
{
    public string Hostname { get; set; } = String.Empty;
    public CoreUserCreateRequest? Admin { get; set; } = new();
    public string JwtSecret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "CoreAccess";
    public string Audience { get; set; } = "CoreAccessClient";
    public string ExpiresIn { get; set; } = "60";
    public string SystemLogLevel { get; set; } = "Information";
    public string DisableRegistration { get; set; } = "false";
}

public class SetupStatus
{
    public bool IsSetupRequired { get; set; } = true;
    public bool IsSetupInProgress { get; set; } = false;
    public int SetupPercentage { get; set; } = 0;
    public string CurrentStep { get; set; } = "Not started";
}