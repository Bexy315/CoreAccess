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
}
