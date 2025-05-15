using CoreAccess.Services;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
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

        services.AddScoped<IUserService, UserService>();
       /**   services.AddScoped<IJwtTokenService, JwtTokenService>(); 
          if (options.EnableRoles)
              services.AddScoped<IRoleService, RoleService>();
 **/

        return services;
    }
}