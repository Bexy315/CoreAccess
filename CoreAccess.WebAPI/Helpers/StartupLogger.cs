using System.Text.RegularExpressions;

namespace CoreAccess.WebAPI.Helpers;

public static class StartupLogger
{
    public static void LogStartupInfo(ILogger logger, WebApplication app)
    {
        IConfiguration config = app.Configuration;
        IWebHostEnvironment env =  app.Environment;
        
        logger.LogInformation("============================================");
        logger.LogInformation("CoreAccess starting up...");
        logger.LogInformation("Environment: {Environment}", env.EnvironmentName);
       
        // Debug Mode
        var debugMode = config.GetValue<bool>("COREACCESS_DEBUGMODE");
        logger.LogInformation("DebugMode: {DebugMode}", debugMode);

        // Database Provider
        var dbProvider = config.GetValue<string>("DatabaseProvider") ?? "SQLite (default)";
        logger.LogInformation("Database Provider: {Provider}", dbProvider);

        var connStr = config.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrEmpty(connStr))
        {
            logger.LogInformation("Database ConnectionString: {Connection}", MaskConnectionString(connStr));
        }

        // JWT Settings (nur Metainfos)
        var jwtIssuer = config.GetValue<string>("Jwt:Issuer");
        logger.LogInformation("JWT Issuer: {Issuer}", jwtIssuer ?? "not set"); 
        
        // 🔹 Adresse und Port nur in Development loggen
        if (env.IsDevelopment())
        {
            var addresses = app.Urls;
            foreach (var address in addresses)
            {
                logger.LogInformation("Listening on: {Address}", address);
            }
        }

        logger.LogInformation("============================================");
    }

    private static string MaskConnectionString(string connStr)
    {
        return Regex.Replace(connStr, "(?<=Password=)[^;]+", "*****", RegexOptions.IgnoreCase);
    }
}
