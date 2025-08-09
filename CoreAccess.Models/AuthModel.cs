namespace CoreAccess.Models;

public class RegisterRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}

public static class AccessClaimType
{
    public const string UserId = "coreaccess:user_id";
    public const string UserName = "coreaccess:username";
    public const string Role = "coreaccess:role";
    public const string TokenId = "coreaccess:token_id";
    public const string Permissions = "coreaccess:permissions";
}