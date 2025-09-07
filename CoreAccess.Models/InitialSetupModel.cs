namespace CoreAccess.Models;

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
    public string Issuer { get; set; } = "coreaccess";
    public string Audience { get; set; } = "coreaccess-client";
    public string ExpiresIn { get; set; } = "3600";
}
public class UserInitialSetupRequest
{
    public UserCreateRequest? Admin { get; set; } = new();
}
