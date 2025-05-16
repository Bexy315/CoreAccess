using CoreAccess.Model;
using Microsoft.EntityFrameworkCore;

namespace CoreAccess;

internal class CoreAccessDbContext : DbContext
{
    public CoreAccessDbContext(DbContextOptions<CoreAccessDbContext> options)
        : base(options) { }

    public DbSet<CoreUser> Users => Set<CoreUser>();
}