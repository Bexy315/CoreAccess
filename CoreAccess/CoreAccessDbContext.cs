using Microsoft.EntityFrameworkCore;

namespace CoreAccess;

public class CoreAccessDbContext : DbContext
{
    public CoreAccessDbContext(DbContextOptions<CoreAccessDbContext> options)
        : base(options) { }

    public DbSet<CoreUser> Users => Set<CoreUser>();
    public DbSet<CoreRole> Roles => Set<CoreRole>();
}

public class CoreUser
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";
}

public class CoreRole
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}