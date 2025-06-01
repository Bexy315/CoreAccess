using System.Text.Json.Serialization;

namespace CoreAccess.WebAPI.Model;

public class CoreLoginRequest
{
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string LoginIp { get; set; } = "0.0.0.0";
}


public class CoreRegisterRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}

public class CoreRefreshTokenRequest
{
    public string RefreshToken { get; set; } = null!;
    public string LoginIp { get; set; } = "0.0.0.0";
}

public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Token { get; set; } = string.Empty;
    public DateTime Expires { get; set; }
    public bool IsExpired => DateTime.UtcNow >= Expires;
    public DateTime Created { get; set; }
    public string CreatedByIp { get; set; } = string.Empty;
    public DateTime? Revoked { get; set; }
    public string? RevokedByIp { get; set; }
    public bool IsActive => Revoked == null && !IsExpired;
    
    public Guid CoreUserId { get; set; }
    [JsonIgnore]
    public CoreUser CoreUser { get; set; } = null!;
}

public static class CoreAccessClaimType
{
    public const string UserId = "coreaccess:user_id";
    public const string UserName = "coreaccess:username";
    public const string Roles = "coreaccess:roles";
    public const string TokenId = "coreaccess:token_id";
    public const string Permissions = "coreaccess:permissions";
}