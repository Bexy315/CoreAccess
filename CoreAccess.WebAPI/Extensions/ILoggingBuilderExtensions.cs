using System.Text.RegularExpressions;

namespace CoreAccess.WebAPI.Extensions;

public static class ILoggingBuilderExtensions
{
    public static ILoggingBuilder AddCoreAccessLogging(this ILoggingBuilder builder, IConfiguration config)
    {
        builder.ClearProviders();
        
        builder.AddConsole();
        
        builder.AddSimpleConsole(options =>
        {
            options.SingleLine = true;
            options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
        });
        
        builder.AddConfiguration(config.GetSection("Logging"));

        return builder;
    }
}