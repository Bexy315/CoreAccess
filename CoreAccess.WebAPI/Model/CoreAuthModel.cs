namespace CoreAccess.WebAPI.Model;

public class CoreLoginRequest
{
    public string? Username { get; set; } = null!;
    public string? Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string LoginIp { get; set; } = "0.0.0.0";
}

public class CoreRefreshTokenRequest
{
    public string RefreshToken { get; set; } = null!;
    public string LoginIp { get; set; } = "0.0.0.0";
}