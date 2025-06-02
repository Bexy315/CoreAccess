namespace CoreAccess.WebAPI.Model;

public enum LogType
{
    System,
    Audit,
    Access
}

public class AuditLogEntry
{
    public string UserId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? Details { get; set; }
}

public class AccessLogEntry
{
    public string UserId { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? Action { get; set; }
    public bool Success { get; set; }
}