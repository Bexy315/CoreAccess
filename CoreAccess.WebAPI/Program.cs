using CoreAccess.BizLayer.Logger;
using CoreAccess.BizLayer.Middleware;
using CoreAccess.BizLayer.Services;
using CoreAccess.DataLayer.DbContext;
using CoreAccess.DataLayer.Repositories;
using CoreAccess.WebAPI.Logger.Sinks;
using dotenv.net;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OpenIddict.Validation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    DotEnv.Load();
    
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("DevCors", policy =>
        {
            policy
                .WithOrigins("http://localhost:8081", "http://localhost:8080")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
    });
}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

#region DbContext

var postgresConnString = Environment.GetEnvironmentVariable("COREACCESS_POSTGRES_CONNECTION");

builder.Services.AddDbContext<CoreAccessDbContext>(options =>
{
    if (!string.IsNullOrWhiteSpace(postgresConnString))
    {
        options.UseNpgsql(postgresConnString);
        Console.WriteLine("PostgreSQL-Verbindung aktiviert.");
    }
    else
    {
        var sqlitePath = builder.Environment.IsDevelopment()
            ? Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "data.sqlite")
            : Path.Combine(AppContext.BaseDirectory, "data.sqlite");

        Directory.CreateDirectory(Path.GetDirectoryName(sqlitePath)!);

        var sqliteConn = $"Data Source={sqlitePath};";
        options.UseSqlite(sqliteConn, x => x.MigrationsAssembly("CoreAccess.DataLayer"));
    }
});

#endregion

#region Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CoreAccess API", Version = "v1" });

    // JWT Bearer Support in Swagger
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
builder.Services.AddOpenApi();

#endregion

#region Repositories

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();

#endregion

#region Services

builder.Services.AddScoped<IAppSettingsService, AppSettingsService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IOpenIddictService, OpenIddictService>();
builder.Services.AddScoped<IInitialSetupService, InitialSetupService>();

#endregion

#region OpenIddict

builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore()
            .UseDbContext<CoreAccessDbContext>();
    })
    .AddServer(options =>
    {
        options.SetTokenEndpointUris("/api/connect/token");
        options.AllowPasswordFlow();
        options.AllowRefreshTokenFlow();
        
        
        options.AcceptAnonymousClients();
        options.UseAspNetCore()
            .EnableTokenEndpointPassthrough()
            .DisableTransportSecurityRequirement();

        options.AddDevelopmentEncryptionCertificate()
            .AddDevelopmentSigningCertificate();
    }).AddValidation(options =>
    {
        options.UseLocalServer();
    });

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
    options.DefaultForbidScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
});

#endregion

var app = builder.Build();

CoreLogger.Initialize(new List<ILogSink>
{
    new ConsoleSink()
});

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CoreAccessDbContext>();
    
    dbContext.Database.Migrate();
}

app.MapOpenApi();
app.UseSwagger();            
app.UseSwaggerUI();

if (app.Environment.IsDevelopment())
{
    app.UseCors("DevCors");
}
else
{
    app.UseDefaultFiles();
    app.UseStaticFiles();   
    
    app.MapFallbackToFile("index.html");
}

app.UseMiddleware<InitialSetupGuardMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();