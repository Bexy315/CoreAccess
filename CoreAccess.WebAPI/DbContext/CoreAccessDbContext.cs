using System.Linq.Expressions;
using CoreAccess.WebAPI.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreAccess.WebAPI.DbContext;

public class CoreAccessDbContext(DbContextOptions<CoreAccessDbContext> options) : Microsoft.EntityFrameworkCore.DbContext(options)
{
    public DbSet<CoreUser> Users { get; set; }
    public DbSet<CoreRole> Roles { get; set; }
    public DbSet<CorePermission> Permissions { get; set; }
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
    userBuilder.HasIndex(u => u.Username)
        .IsUnique();

    userBuilder.Property(u => u.PasswordHash)
        .IsRequired()
        .HasMaxLength(255);

    userBuilder.HasMany(u => u.Roles)
        .WithMany(r => r.Users)
        .UsingEntity<Dictionary<string, object>>(
            "UserRoles",
            j => j.HasOne<CoreRole>().WithMany().HasForeignKey("RoleId"),
            j => j.HasOne<CoreUser>().WithMany().HasForeignKey("UserId"),
            j =>
            {
                j.ToTable("UserRoles");
                j.HasKey("UserId", "RoleId");
                j.HasIndex("RoleId");
            });

    userBuilder.HasMany(u => u.RefreshTokens)
        .WithOne(rt => rt.CoreUser)
        .HasForeignKey(rt => rt.CoreUserId)
        .OnDelete(DeleteBehavior.Restrict);

    #endregion

    #region RefreshToken
    var tokenBuilder = modelBuilder.Entity<RefreshToken>();
    tokenBuilder.HasKey(rt => rt.Id);
    tokenBuilder.HasIndex(rt => rt.CoreUserId);
    
    tokenBuilder.HasIndex(rt => rt.Token).IsUnique();

    if (isSqlite)
    {
        ConfigureGuidAsBlob(tokenBuilder, rt => rt.Id);
        ConfigureGuidAsBlob(tokenBuilder, rt => rt.CoreUserId);
    }
    #endregion

    #region CoreRole
    var roleBuilder = modelBuilder.Entity<CoreRole>();
    roleBuilder.HasKey(r => r.Id);

    roleBuilder.HasMany(r => r.Permissions)
        .WithMany(p => p.Roles)
        .UsingEntity<Dictionary<string, object>>(
            "RolePermissions",
            j => j.HasOne<CorePermission>().WithMany().HasForeignKey("PermissionId"),
            j => j.HasOne<CoreRole>().WithMany().HasForeignKey("RoleId"),
            j =>
            {
                j.ToTable("RolePermissions");
                j.HasKey("RoleId", "PermissionId");
                j.HasIndex("PermissionId");
            });

    if (isSqlite) ConfigureGuidAsBlob(roleBuilder, r => r.Id);
    #endregion

    #region CorePermission
    var permissionBuilder = modelBuilder.Entity<CorePermission>();
    permissionBuilder.HasKey(p => p.Id);
    #endregion

    #region AppSetting
    var settingBuilder = modelBuilder.Entity<AppSetting>();
    settingBuilder.HasKey(s => s.Id);
    
    settingBuilder.HasIndex(s => s.Key).IsUnique();

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
