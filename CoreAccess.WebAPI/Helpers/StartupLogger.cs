using System.Text.RegularExpressions;
using CoreAccess.BizLayer.Services;
using CoreAccess.Models;

namespace CoreAccess.WebAPI.Helpers;

public static class StartupLogger
{
    public static void LogStartupInfo(ILogger logger, WebApplication app)
    {
        IWebHostEnvironment env =  app.Environment;
        
        logger.LogInformation("============================================");
        logger.LogInformation("CoreAccess starting up...");
        logger.LogInformation("Environment: {Environment}", env.EnvironmentName);
       
        // Debug Mode
        var debugMode = Environment.GetEnvironmentVariable("COREACCESS_DEBUGMODE")?? "False";
        logger.LogInformation("DebugMode: {DebugMode}", debugMode);
        

        logger.LogInformation("============================================");
    }
}
