using System.Linq.Expressions;
using CoreAccess.WebAPI.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreAccess.WebAPI.DbContext;

public class CoreAccessDbContext(DbContextOptions<CoreAccessDbContext> options) : Microsoft.EntityFrameworkCore.DbContext(options)
{
    public DbSet<CoreUser> Users { get; set; }
    public DbSet<CoreRole> Roles { get; set; }
    public DbSet<AppSetting> AppSettings { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("coreaccess");

        var isSqlite = Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite";

        #region CoreUser
        var userBuilder = modelBuilder.Entity<CoreUser>();

        userBuilder.HasKey(u => u.Id);
        if (isSqlite) ConfigureGuidAsBlob(userBuilder, u => u.Id);

        userBuilder.Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(50);

        userBuilder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(255);

        userBuilder.HasMany(u => u.Roles)
            .WithMany(r => r.Users)
            .UsingEntity(j => j.ToTable("UserRoles"));

        userBuilder.HasMany(u => u.RefreshTokens)
            .WithOne(rt => rt.CoreUser)
            .HasForeignKey(rt => rt.CoreUserId)
            .OnDelete(DeleteBehavior.Restrict);

        #endregion

        #region RefreshToken
        var tokenBuilder = modelBuilder.Entity<RefreshToken>();
        tokenBuilder.HasKey(rt => rt.Id);
        if (isSqlite)
        {
            ConfigureGuidAsBlob(tokenBuilder, rt => rt.Id);
            ConfigureGuidAsBlob(tokenBuilder, rt => rt.CoreUserId);
        }
        #endregion

        #region CoreRole
        var roleBuilder = modelBuilder.Entity<CoreRole>();
        roleBuilder.HasKey(r => r.Id);
        if (isSqlite) ConfigureGuidAsBlob(roleBuilder, r => r.Id);
        
        roleBuilder.HasData(
            new CoreRole
            {
                Id = Guid.Parse("f63ee75d-f899-4fe5-ae6d-a070164d01fd"),
                Name = "Admin",
                Description = "Admin role for Administrators",
                CreatedAt = "2025-01-01T00:00:00Z",
                UpdatedAt = "2025-01-01T00:00:00Z",
                IsSystem = true
            },
            new CoreRole
            {
                Id = Guid.Parse("7bd719cd-fdcc-4463-b633-2aef7208ba38"),
                Name = "User",
                Description = "User role for Default Users",
                CreatedAt = "2025-01-01T00:00:00Z",
                UpdatedAt = "2025-01-01T00:00:00Z",
                IsSystem = true
            }
        );
        #endregion

        #region AppSetting
        var settingBuilder = modelBuilder.Entity<AppSetting>();
        settingBuilder.HasKey(s => s.Id);
        if (isSqlite) ConfigureGuidAsBlob(settingBuilder, s => s.Id);
        #endregion

        base.OnModelCreating(modelBuilder);
    }

    private static void ConfigureGuidAsBlob<T>(EntityTypeBuilder<T> builder, Expression<Func<T, Guid>> property)
        where T : class
    {
        builder.Property(property)
            .HasConversion(
                guid => guid.ToByteArray(),
                bytes => new Guid(bytes))
            .HasColumnType("BLOB");
    }
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        PreventSystemDeletes();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void PreventSystemDeletes()
    {
        var deletedUsers = ChangeTracker.Entries<CoreUser>()
            .Where(e => e.State == EntityState.Deleted && e.Entity.IsSystem)
            .ToList();

        if (deletedUsers.Any())
        {
            throw new InvalidOperationException("Systembenutzer dürfen nicht gelöscht werden.");
        }
    }

}
