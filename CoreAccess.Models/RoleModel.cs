using System.Text.Json.Serialization;

namespace CoreAccess.Models;

public class Role
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string CreatedAt { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
    public string UpdatedAt { get; set; } = "";
    public bool IsSystem { get; set; } = false;
    public List<Permission> Permissions { get; set; } = new();

    [JsonIgnore]
    public List<User> Users { get; set; }
}

public class RoleDto()
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
}

public class RoleDetailDto : RoleDto
{
    public string CreatedAt { get; set; }
    public string UpdatedAt { get; set; }
    public bool IsSystem { get; set; }
    public List<PermissionDto> Permissions { get; set; }
    public List<UserDto> Users { get; set; } 
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
    public bool? IncludeUsers { get; set; } = false;
    public bool? IncludePermissions = false;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}