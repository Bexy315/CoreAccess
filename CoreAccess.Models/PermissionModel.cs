using System.Text.Json.Serialization;

namespace CoreAccess.Models;

public class Permission
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string CreatedAt { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
    public string UpdatedAt { get; set; } = "";
    public bool IsSystem { get; set; } = false;
    [JsonIgnore]
    public List<Role> Roles { get; set; } = new();
}

public class PermissionDto(Permission src)
{
    public Guid Id { get; set; } = src.Id;
    public string Name { get; set; } = src.Name;
    public string? Description { get; set; } = src.Description;
}

public class PermissionDetailsDto(Permission src) : PermissionDto(src)
{
    public string CreatedAt { get; set; } = src.CreatedAt;
    public string UpdatedAt { get; set; } = src.UpdatedAt;
    public bool IsSystem { get; set; } = src.IsSystem;
    public List<RoleDto> Roles { get; set; } = src.Roles.Select(r => new RoleDto(r)).ToList();
}

public class PermissionSearchOptions
{
    public string? Search { get; set; }
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsSystem { get; set; }
    public List<string> Roles { get; set; } = new();
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class CreatePermissionRequest
{
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public bool IsSystem { get; set; } = false;
}