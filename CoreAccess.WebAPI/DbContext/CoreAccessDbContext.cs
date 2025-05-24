using CoreAccess.WebAPI.Model;
using Microsoft.EntityFrameworkCore;

namespace CoreAccess.WebAPI.DbContext;

public class CoreAccessDbContext(DbContextOptions<CoreAccessDbContext> options) : Microsoft.EntityFrameworkCore.DbContext(options)
{
    public DbSet<CoreUser> Users { get; set; }
    public DbSet<CoreRole> Roles { get; set; }
    public DbSet<AppSetting> AppSettings { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("coreaccess");
        
        #region CoreUser
        modelBuilder.Entity<CoreUser>()
            .HasKey(u => u.Id);
        
        modelBuilder.Entity<CoreUser>()
            .Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(50);

        modelBuilder.Entity<CoreUser>()
            .Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(255);

        modelBuilder.Entity<CoreUser>()
            .HasMany(u => u.Roles)
            .WithMany(r => r.Users)
            .UsingEntity(j => j.ToTable("UserRoles"));
        #endregion
        #region CoreRole
        modelBuilder.Entity<CoreRole>()
            .HasKey(r => r.Id);
        #endregion
        #region AppSetting
        modelBuilder.Entity<AppSetting>()
            .HasKey(s => s.Id);
        #endregion
        
        base.OnModelCreating(modelBuilder);
    }
}
