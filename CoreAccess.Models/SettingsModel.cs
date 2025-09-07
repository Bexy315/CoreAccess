namespace CoreAccess.Models;

public class Setting
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Key { get; set; } = default!;
    public string? Value { get; set; }           // Klartext (nicht geheim)
    public string? EncryptedValue { get; set; } // Verschlüsselt (wenn geheim)
    public bool IsSecret { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class SettingSearchOptions
{
    public string? Key { get; set; }
    public bool? IsSecret { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public static class SettingsKeys
{
    public const string BaseUri = "CoreAccess:BaseUri";
    public const string InitSetupCompleted = "CoreAccess:InitSetupCompleted";
    public const string JwtIssuer = "CoreAccess:Jwt:Issuer";
    public const string JwtAudience = "CoreAccess:Jwt:Audience";
    public const string JwtExpiresIn = "CoreAccess:Jwt:ExpiresIn";
    public const string DisableRegistration = "CoreAccess:Common:DisableRegistration";
    public const string SystemLogLevel = "CoreAccess:Logging:SystemLogLevel";
}