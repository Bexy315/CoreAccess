using CoreAccess.WebAPI.DbContext;
using CoreAccess.WebAPI.Repositories;
using CoreAccess.WebAPI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

#region Repositories

builder.Services.AddScoped<IUserRepository, UserRepository>();

#endregion

#region Services

builder.Services.AddScoped<IUserService, UserService>();

#endregion

#region DbContext

var postgresConnString = Environment.GetEnvironmentVariable("COREACCESS_POSTGRES_CONNECTION");

builder.Services.AddDbContext<CoreAccessDbContext>(options =>
{
    if (!string.IsNullOrWhiteSpace(postgresConnString))
    {
        options.UseNpgsql(postgresConnString);
        Console.WriteLine("✅ PostgreSQL-Verbindung aktiviert.");
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
            ? "🔄 Verwende SQLite als lokale Datenbank in Entwicklungsumgebung."
            : "🔄 Verwende SQLite als lokale Datenbank.");
    }
});

#endregion

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();            
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();