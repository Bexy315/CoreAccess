namespace CoreAccess;

public enum DatabaseType
{
    SQLiteFile,
    PostgreSQL
}

public class CoreAccessOptions
{
    public DatabaseType DatabaseType { get; set; }
    public string? ConnectionString { get; set; }

    public bool EnableRoles { get; set; } = true;
    public string[] AllowedRoles { get; set; } = Array.Empty<string>();

    public string JwtSecret { get; set; } = string.Empty;
    public string JwtIssuer { get; set; } = "CoreAccess";
    public int JwtExpiryMinutes { get; set; } = 60;

    public bool EnableUserLockout { get; set; } = false;
    public int MaxFailedLogins { get; set; } = 5;

    public void UseDatabase(DatabaseType type, string connectionString)
    {
        DatabaseType = type;
        ConnectionString = connectionString;
    }
}