namespace CoreAccess.Models.Extensions;

public static class UserModelExtensions
{
    public static UserDto ToDto(this User entity) =>
        new UserDto
        {
            Id = entity.Id,
            Username = entity.Username,
            Email = entity.Email,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            Status = entity.Status
        };

    public static UserDetailDto ToDetailDto(this User entity) =>
        new UserDetailDto
        {
            Id = entity.Id,
            Username = entity.Username,
            Email = entity.Email,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            Phone = entity.Phone,
            Address = entity.Address,
            City = entity.City,
            State = entity.State,
            Zip = entity.Zip,
            Country = entity.Country,
            ProfilePicture = entity.ProfilePicture,
            ProfilePictureContentType = entity.ProfilePictureContentType,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            Roles = entity.Roles?.Select(r => r.ToDto()).ToList() ?? new List<RoleDto>()
        };
}