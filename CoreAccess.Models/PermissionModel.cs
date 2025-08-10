using System.Text.Json.Serialization;

namespace CoreAccess.Models;

public class Permission
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string CreatedAt { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
    public string UpdatedAt { get; set; } = "";
    public bool IsSystem { get; set; } = false;
    [JsonIgnore]
    public Tenant Tenant { get; set; }
    [JsonIgnore]
    public List<Role> Roles { get; set; } = new();
}

public class CreatePermissionRequest
{
    public Guid TenantId { get; set; } = Guid.Empty;
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public bool IsSystem { get; set; } = false;
}