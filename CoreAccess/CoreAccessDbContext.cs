using CoreAccess.Model;
using Microsoft.EntityFrameworkCore;

namespace CoreAccess;

public class CoreAccessDbContext : DbContext
{
    public CoreAccessDbContext(DbContextOptions<CoreAccessDbContext> options)
        : base(options) { }

    public DbSet<CoreUser> Users => Set<CoreUser>();
}