using CoreAccess.BizLayer.Logger;
using CoreAccess.BizLayer.Middleware;
using CoreAccess.BizLayer.Services;
using CoreAccess.DataLayer.DbContext;
using CoreAccess.DataLayer.Repositories;
using CoreAccess.WebAPI.Extensions;
using CoreAccess.WebAPI.Logger.Sinks;
using CoreAccess.Workers;
using dotenv.net;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    DotEnv.Load();
}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddRazorPages();

builder.Services
    .AddCoreAccessCors()
    .AddCoreAccessDbContext()
    .AddCoreAccessSwagger()
    .AddCoreAccessRepositories()
    .AddCoreAccessServices()
    .AddCoreAccessOpenIddict();

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

if (app.Environment.IsDevelopment() || 
    Environment.GetEnvironmentVariable("COREACCESS_DEBUGMODE") == "True")
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

app.MapRazorPages();

app.MapControllers();

app.Run();