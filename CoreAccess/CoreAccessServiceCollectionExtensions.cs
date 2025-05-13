using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CoreAccess;

public static class CoreAccessServiceCollectionExtensions
{
    public static IServiceCollection AddCoreAccess(this IServiceCollection services, Action<CoreAccessOptions> configure)
    {
        var options = new CoreAccessOptions();
        configure(options);

        services.AddSingleton(options);

        if (options.DatabaseType == DatabaseType.SQLiteFile)
        {
            services.AddDbContext<CoreAccessDbContext>(opt =>
                opt.UseSqlite(options.ConnectionString));
        }
        else if (options.DatabaseType == DatabaseType.PostgreSQL)
        {
            services.AddDbContext<CoreAccessDbContext>(opt =>
                opt.UseNpgsql(options.ConnectionString));
        }

     /**   services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IUserService, UserService>();
        if (options.EnableRoles)
            services.AddScoped<IRoleService, RoleService>();

        services.AddControllers().PartManager.ApplicationParts.Add(
            new AssemblyPart(typeof(CoreAccessServiceCollectionExtensions).Assembly)); **/

        return services;
    }
}