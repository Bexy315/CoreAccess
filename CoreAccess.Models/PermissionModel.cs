using System.Text.Json.Serialization;
using CoreAccess.Models.Extensions;

namespace CoreAccess.Models;

public class Permission
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string CreatedAt { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
    public string UpdatedAt { get; set; } = "";
    public bool IsSystem { get; set; } = false;
    [JsonIgnore]
    public List<Role> Roles { get; set; } = new();
}

public class PermissionDto()
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
}

public class PermissionDetailDto() : PermissionDto()
{
    public string CreatedAt { get; set; }
    public string UpdatedAt { get; set; } 
    public bool IsSystem { get; set; }
    public List<RoleDto> Roles { get; set; }
}

public class PermissionSearchOptions
{
    public string? Search { get; set; }
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsSystem { get; set; }
    public bool? IncludeRoles { get; set; }
    public List<string> Roles { get; set; } = new();
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class PermissionCreateRequest
{
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public bool IsSystem { get; set; } = false;
}
public class PermissionUpdateRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
}