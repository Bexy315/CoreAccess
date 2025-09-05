namespace CoreAccess.Models.Extensions;

public static class RoleModelExtensions
{
    public static RoleDto ToDto(this Role entity) =>
        new RoleDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description
        };

    public static RoleDetailDto ToDetailDto(this Role entity) =>
        new RoleDetailDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            IsSystem = entity.IsSystem,
            Permissions = entity.Permissions.Select(p => p.ToDto()).ToList(),
            Users = entity.Users.Select(u => u.ToDto()).ToList()
        };
}