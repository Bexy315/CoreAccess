using CoreAccess.WebAPI.Model;
using Microsoft.EntityFrameworkCore;

namespace CoreAccess.WebAPI.DbContext;

public class AppDbContext(DbContextOptions<AppDbContext> options) : Microsoft.EntityFrameworkCore.DbContext(options)
{
    public DbSet<CoreUser> Users { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CoreUser>()
            .Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(50);

        modelBuilder.Entity<CoreUser>()
            .Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<CoreUser>()
            .Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(255);

        base.OnModelCreating(modelBuilder);
    }
}
