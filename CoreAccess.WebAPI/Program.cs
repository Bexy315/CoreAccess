using CoreAccess.BizLayer.Middleware;
using CoreAccess.BizLayer.Services;
using CoreAccess.DataLayer.DbContext;
using CoreAccess.WebAPI.Extensions;
using CoreAccess.WebAPI.Helpers;
using dotenv.net;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    DotEnv.Load();
}

builder.Logging.AddCoreAccessLogging(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddRazorPages();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDataProtection();
}else
{
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo("/data/keys/dpKeys/"))
        .SetApplicationName("CoreAccess");
}

builder.Services.AddMemoryCache();

builder.Services
    .AddCoreAccessCors()
    .AddCoreAccessDbContext()
    .AddCoreAccessSwagger()
    .AddCoreAccessRepositories()
    .AddCoreAccessServices()
    .AddCoreAccessOpenIddict();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    StartupLogger.LogStartupInfo(logger, app);
}

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

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<InitialSetupGuardMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.MapControllers();

app.Run();