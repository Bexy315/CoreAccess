using System.Text.Json.Serialization;

namespace CoreAccess.Models;

public enum UserStatus
{
    Active,
    Inactive,
    Suspended,
    Deleted
}

public class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Username { get; set; } = "";
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Zip { get; set; }
    public string? Country { get; set; }
    public byte[]? ProfilePicture { get; set; } 
    public string? ProfilePictureContentType { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Active;
    public bool IsSystem { get; set; } = false;
    public string CreatedAt { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
    public string UpdatedAt { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public List<Role> Roles { get; set; } = new();
}

public class UserDto
{
    public string Id { get; set; } 
    public string Username { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public UserStatus Status { get; set; }
}

public class UserDetailDto : UserDto
{
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Zip { get; set; }
    public string? Country { get; set; }
    public byte[]? ProfilePicture { get; set; }
    public string? ProfilePictureContentType { get; set; }
    public string CreatedAt { get; set; }
    public string UpdatedAt { get; set; }
    public List<RoleDto> Roles { get; set; }
}

public class UserCreateRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Zip { get; set; }
    public string? Country { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Active;
}

public class UserUpdateRequest
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Zip { get; set; }
    public string? Country { get; set; }
    public UserStatus? Status { get; set; }
}

public class UserSearchOptions
{
    public string? Search { get; set; } = "";
    public string? Id { get; set; }
    public string? Username { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Zip { get; set; }
    public string? Country { get; set; }
    public UserStatus[]? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}