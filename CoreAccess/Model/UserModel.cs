namespace CoreAccess.Model;

public class CoreUser
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";
}

public class CoreUserCreateRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}

public class CoreUserUpdateRequest
{
    public string? Username { get; set; } = "";
}

public class CoreUserSearchOptions
{
    public string? Id { get; set; }
    public string? Name { get; set; }
}