using System.Text.Json.Serialization;

namespace CoreAccess.WebAPI.Model;

public class LoginRequest
{
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string LoginIp { get; set; } = "0.0.0.0";
}

public class LoginResponse
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public Guid UserId { get; set; }
}


public class RegisterRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}

public class RefreshTokenRequest
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
    public DateTime CreatedAt { get; set; }
    public DateTime? Revoked { get; set; }
    public bool IsActive => Revoked == null && !IsExpired;
    public Guid CoreUserId { get; set; }
    [JsonIgnore]
    public User User { get; set; } = null!;
}

public class RefreshTokenResponse
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public Guid UserId { get; set; }
}

public static class AccessClaimType
{
    public const string UserId = "coreaccess:user_id";
    public const string UserName = "coreaccess:username";
    public const string Roles = "coreaccess:roles";
    public const string TokenId = "coreaccess:token_id";
    public const string Permissions = "coreaccess:permissions";
}