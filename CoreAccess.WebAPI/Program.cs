using CoreAccess.WebAPI.DbContext;
using CoreAccess.WebAPI.Repositories;
using CoreAccess.WebAPI.Services;
using dotenv.net;
using Microsoft.EntityFrameworkCore;
using CoreAccess.WebAPI.Logger;
using CoreAccess.WebAPI.Logger.Sinks;
using CoreAccess.WebAPI.Middleware;
using Microsoft.OpenApi.Models;

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
        options.UseSqlite(sqliteConn);
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

builder.Services.AddScoped<InitialSetupService>();

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
        options.SetTokenEndpointUris("/connect/token");
        options.AllowPasswordFlow();
        options.AllowRefreshTokenFlow();
        
        
        options.AcceptAnonymousClients();
        options.UseAspNetCore()
            .EnableTokenEndpointPassthrough()
            .DisableTransportSecurityRequirement();

        options.AddDevelopmentEncryptionCertificate()
            .AddDevelopmentSigningCertificate();
    });

#endregion


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CoreAccessDbContext>();
    dbContext.Database.Migrate();
}

CoreLogger.Initialize(new List<ILogSink>
{
    new ConsoleSink()
});

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

app.UseAuthorization();

app.MapControllers();

app.Run();