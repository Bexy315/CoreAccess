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
    public Guid Id { get; set; } = Guid.NewGuid();
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

public class UserDto(User src)
{
    public Guid Id { get; set; } = src.Id;
    public string Username { get; set; } = src.Username;
    public string? Email { get; set; } = src.Email;
    public string? FirstName { get; set; } = src.FirstName;
    public string? LastName { get; set; } = src.LastName;
    public string? Phone { get; set; } = src.Phone;
    public string? Address { get; set; } = src.Address;
    public string? City { get; set; } = src.City;
    public string? State { get; set; } = src.State;
    public string? Zip { get; set; } = src.Zip;
    public string? Country { get; set; } = src.Country;
    public byte[]? ProfilePicture { get; set; } = src.ProfilePicture;
    public string? ProfilePictureContentType { get; set; } = src.ProfilePictureContentType;
    public UserStatus Status { get; set; } = src.Status;
    public string CreatedAt { get; set; } = src.CreatedAt;
    public string UpdatedAt { get; set; } = src.UpdatedAt;
    public List<Role> Roles { get; set; } = src.Roles;
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
    public UserStatus? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}