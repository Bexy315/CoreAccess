namespace CoreAccess.WebAPI.Model;

public class AppSetting
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Key { get; set; } = default!;
    public string Value { get; set; } = default!;
    public bool IsEncrypted { get; set; }
    public bool IsSystem { get; set; } = false;
}
public class AppSettingDto
{
    public string Key { get; set; } = default!;
    public string Value { get; set; } = default!;
    public bool IsEncrypted { get; set; }
    public bool IsSystem { get; set; } = false;

    public AppSettingDto() { }

    public AppSettingDto(AppSetting setting)
    {
        Key = setting.Key;
        Value = setting.Value;
        IsEncrypted = setting.IsEncrypted;
        IsSystem = setting.IsSystem;
    }
}
public class AppSettingSearchOptions
{
    public string? Key { get; set; }
    public string? Value { get; set; }
    public bool IncludeSystemSettings { get; set; } = true;
    public bool IncludeEncryptedSettings { get; set; } = true;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class AppSettingCreateRequest
{
    public string Key { get; set; } = default!;
    public string Value { get; set; } = default!;
    public bool IsEncrypted { get; set; }
    public bool IsSystem { get; set; } = false;
}

public class AppSettingUpdateRequest
{
    public string? Key { get; set; }
    public string? Value { get; set; }
    public bool? IsEncrypted { get; set; }
    public bool? IsSystem { get; set; }
}

public static class AppSettingsKeys
{
    public const string BaseUri = "CoreAccess:BaseUri";
    public const string InitSetupCompleted = "CoreAccess:InitSetupCompleted";
    public const string JwtSecretKey = "CoreAccess:Jwt:Secret";
    public const string JwtIssuer = "CoreAccess:Jwt:Issuer";
    public const string JwtAudience = "CoreAccess:Jwt:Audience";
    public const string JwtExpiresIn = "CoreAccess:Jwt:ExpiresIn";
    public const string DisableRegistration = "CoreAccess:Common:DisableRegistration";
    public const string SystemLogLevel = "CoreAccess:Logging:SystemLogLevel";
}