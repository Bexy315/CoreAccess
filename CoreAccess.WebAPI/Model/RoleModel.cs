namespace CoreAccess.WebAPI.Model;

public class CoreRole
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string CreatedAt { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
    public string UpdatedAt { get; set; } = "";
    public bool IsSystem { get; set; } = false;
    public List<string>? Permissions { get; set; }
    public List<CoreUser> Users { get; set; } = new();
}

public class CoreRoleDto(CoreRole src)
{
    public Guid Id { get; set; } = src.Id;
    public string Name { get; set; } = src.Name;
    public string? Description { get; set; } = src.Description;
    public string CreatedAt { get; set; } = src.CreatedAt;
    public string UpdatedAt { get; set; } = src.UpdatedAt;
    public bool IsSystem { get; set; } = src.IsSystem;
    public List<string>? Permissions { get; set; } = src.Permissions;
}
public class CoreRoleCreateRequest
{
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public List<string>? Permissions { get; set; }
}
public class CoreRoleUpdateRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public List<string>? Permissions { get; set; }
}
public class CoreRoleSearchOptions
{
    public string? Search { get; set; }
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsSystem { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}