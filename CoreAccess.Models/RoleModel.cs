using System.Text.Json.Serialization;

namespace CoreAccess.Models;

public class Role
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string CreatedAt { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
    public string UpdatedAt { get; set; } = "";
    public bool IsSystem { get; set; } = false;
    public List<Permission> Permissions { get; set; } = new();

    [JsonIgnore]
    public List<User> Users { get; set; }
}

public class RoleDto(Role src)
{
    public Guid Id { get; set; } = src.Id;
    public string Name { get; set; } = src.Name;
    public string? Description { get; set; } = src.Description;
    public string CreatedAt { get; set; } = src.CreatedAt;
    public string UpdatedAt { get; set; } = src.UpdatedAt;
    public bool IsSystem { get; set; } = src.IsSystem;
}
public class RoleCreateRequest
{
    public string Name { get; set; } = "";
    public string? Description { get; set; }
}
public class RoleUpdateRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
}
public class RoleSearchOptions
{
    public string? Search { get; set; }
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsSystem { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}