using CoreAccess.WebAPI.DbContext;
using CoreAccess.WebAPI.Repositories;
using CoreAccess.WebAPI.Services;
using dotenv.net;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CoreAccess.WebAPI.Helpers;
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
                .WithOrigins("http://localhost:8080", "http://localhost:8081", "http://localhost:8082")
                .AllowAnyMethod()
                .AllowAnyHeader();
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
        Console.WriteLine(builder.Environment.IsDevelopment()
            ? "Verwende SQLite als lokale Datenbank in Entwicklungsumgebung."
            : "Verwende SQLite als lokale Datenbank.");
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
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

#endregion

#region Services

builder.Services.AddScoped<ICoreAccessTokenService, CoreAccessTokenService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();

#endregion

#region Auth

var jwtSecret = Environment.GetEnvironmentVariable("COREACCESS_JWT_SECRET") ?? "default_secret_key_change_me";
var key = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // Set to true in production!
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

#endregion

var app = builder.Build();

if (!builder.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<CoreAccessDbContext>();
    dbContext.Database.Migrate();
}

AppSettingsHelper.Initialize(app.Services);

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CoreAccessDbContext>();
    await CoreAccessDbSeeder.SeedInitialDataAsync(db);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();            
    app.UseSwaggerUI();
    app.UseCors("DevCors");
}
else
{
    app.UseDefaultFiles();
    app.UseStaticFiles();   
    
    // Fallback für SPA-Routing
    app.MapFallbackToFile("index.html");
}

app.UseAuthorization();

app.MapControllers();

app.Run();