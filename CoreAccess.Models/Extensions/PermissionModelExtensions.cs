namespace CoreAccess.Models.Extensions;

public static class PermissionModelExtensions
{
    public static PermissionDto ToDto(this Permission entity) =>
        new PermissionDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description
        };

    public static PermissionDetailDto ToDetailDto(this Permission entity) =>
        new PermissionDetailDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            IsSystem = entity.IsSystem,
            Roles = entity.Roles?.Select(r => r.ToDto()).ToList() ?? new List<RoleDto>()
        };
}