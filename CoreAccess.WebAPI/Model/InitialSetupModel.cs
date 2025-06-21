namespace CoreAccess.WebAPI.Model;

public class InitialSetupRequest
{
    public GeneralInitialSetupRequest GeneralInitialSettings { get; set; } = new ();
    public JwtInitialSetupRequest JwtInitialSettings { get; set; } = new ();
    public UserInitialSetupRequest UserInitialSettings { get; set; } = new ();
}
public class GeneralInitialSetupRequest
{
    public string BaseUri { get; set; } = String.Empty;
    public string SystemLogLevel { get; set; } = "Information";
    public string DisableRegistration { get; set; } = "false";
}
public class JwtInitialSetupRequest
{
    public string JwtSecret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "CoreAccess";
    public string Audience { get; set; } = "CoreAccessClient";
    public string ExpiresIn { get; set; } = "60";
}
public class UserInitialSetupRequest
{
    public CoreUserCreateRequest? Admin { get; set; } = new();
}
