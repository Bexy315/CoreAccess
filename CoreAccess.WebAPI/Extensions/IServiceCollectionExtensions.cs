using System.Security.Cryptography;
using CoreAccess.BizLayer.Helpers;
using CoreAccess.BizLayer.Services;
using CoreAccess.DataLayer.DbContext;
using CoreAccess.DataLayer.Repositories;
using CoreAccess.Models;
using CoreAccess.WebAPI.Helpers;
using CoreAccess.Workers;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenIddict.Server;

namespace CoreAccess.WebAPI.Extensions;


public static class IServiceCollectionExtensions
{
    private static readonly string Environment = System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")??"Production";
    private static readonly bool IsDevelopment = Environment == "Development";
    
public static IServiceCollection AddCoreAccessCors(this IServiceCollection services)
    {
        if (IsDevelopment)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("DevCors", policy =>
                {
                    policy
                        .WithOrigins("http://localhost:8081", "http://localhost:8080", "http://localhost:5173")
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
            });
        }

        return services;
    }

    public static IServiceCollection AddCoreAccessDbContext(this IServiceCollection services)
    {
        var postgresConnString = System.Environment.GetEnvironmentVariable("COREACCESS_POSTGRES_CONNECTION");

        services.AddDbContext<CoreAccessDbContext>(options =>
        {
            if (!string.IsNullOrWhiteSpace(postgresConnString))
            {
                options.UseNpgsql(postgresConnString);
                Console.WriteLine("PostgreSQL-Verbindung aktiviert.");
            }
            else
            {
                var sqlitePath = IsDevelopment
                    ? Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "data.sqlite")
                    : Path.Combine(AppContext.BaseDirectory + "/data", "data.sqlite");

                Directory.CreateDirectory(Path.GetDirectoryName(sqlitePath)!);

                var sqliteConn = $"Data Source={sqlitePath};";
                options.UseSqlite(sqliteConn, x => x.MigrationsAssembly("CoreAccess.DataLayer"));
            }
        });

        return services;
    }

    public static IServiceCollection AddCoreAccessSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "CoreAccess API", Version = "v1" });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme.\r\n\r\n" +
                              "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\n" +
                              "Example: \"Bearer abc123\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement()
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header
                    },
                    new List<string>()
                }
            });
        });

        services.AddOpenApi();

        return services;
    }

    public static IServiceCollection AddCoreAccessRepositories(this IServiceCollection services)
    {
        services.AddScoped<ISettingsRepository, SettingsRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();

        return services;
    }

    public static IServiceCollection AddCoreAccessServices(this IServiceCollection services)
    {
        services.AddScoped<ISettingsService, SettingsService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IOpenIddictService, OpenIddictService>();
        services.AddScoped<IInitialSetupService, InitialSetupService>();
        
        services.AddSingleton<ISecretProtector, SecretProtector>();

        services.AddHostedService<CommonWorkerService>();

        return services;
    }

    public static IServiceCollection AddCoreAccessOpenIddict(this IServiceCollection services)
    {
        services.AddAuthentication("Cookies")
            .AddCookie("Cookies", options =>
            {
                options.LoginPath = "/account/login";
                options.LogoutPath = "/account/logout";
            });

        services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                    .UseDbContext<CoreAccessDbContext>();
            })
            .AddServer(options =>
            {
                options.SetTokenEndpointUris("/connect/token");
                options.SetAuthorizationEndpointUris("/connect/authorize");
                options.SetUserInfoEndpointUris("/connect/userinfo");
                options.SetIntrospectionEndpointUris("/connect/introspect");
                options.SetRevocationEndpointUris("/connect/revoke");
                options.SetEndSessionEndpointUris("/connect/endsession");

                options.AllowPasswordFlow();
                options.AllowRefreshTokenFlow();
                options.AllowAuthorizationCodeFlow();

                options.UseAspNetCore()
                    .DisableTransportSecurityRequirement()
                    .EnableTokenEndpointPassthrough()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableUserInfoEndpointPassthrough()
                    .EnableEndSessionEndpointPassthrough();

                if (IsDevelopment)
                {
                    options.AddDevelopmentEncryptionCertificate()
                        .AddDevelopmentSigningCertificate();
                }else
                {
                    var encryptionKey = SecureKeyHelper.LoadOrCreateRsaKey("encryption_key");
                    var signingKey = SecureKeyHelper.LoadOrCreateRsaKey("signing_key");
                    options.AddEncryptionKey(encryptionKey);
                    options.AddSigningKey(signingKey);
                }

                options.RegisterClaims("role", "email", "name");
            })
            .AddValidation(options =>
            {
                options.UseLocalServer();
            });

        return services;
    }
}
