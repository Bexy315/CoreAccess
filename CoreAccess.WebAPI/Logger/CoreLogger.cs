using CoreAccess.WebAPI.Helpers;
using CoreAccess.WebAPI.Logger.Model;
using CoreAccess.WebAPI.Logger.Sinks;
using CoreAccess.WebAPI.Model;

namespace CoreAccess.WebAPI.Logger;

public static class CoreLogger
{
    private static List<ILogSink> _sinks;
    
    public static void Initialize(IEnumerable<ILogSink> sinks)
    {
        _sinks = sinks.ToList();
    }
    
    public static void LogSystem(CoreLogLevel level,string source, string message, Exception? exception = null)
    {
        if (_sinks == null || !_sinks.Any())
        {
            throw new InvalidOperationException("Logger not initialized. Call Initialize() before logging.");
        }
        
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Message cannot be null or empty.", nameof(message));
        }
        
        if (level < CoreLogLevel.Debug || level > CoreLogLevel.Critical)
        {
            throw new ArgumentOutOfRangeException(nameof(level), "Invalid log level.");
        }
        
        AppSettingsHelper.TryGet(AppSettingsKeys.SystemLogLevel, out string? currentLogLevel);
        
        if (!Enum.TryParse(currentLogLevel, out CoreLogLevel parsedLogLevel))
        {
            parsedLogLevel = CoreLogLevel.Information; // Fallback oder gewünschter Standardwert
        }
        
        if(level < parsedLogLevel)
        {
            return; // Log level is lower than the configured level, skip logging
        }
        
        foreach (var sink in _sinks)
        {
            sink.LogSystem(level, source, message, exception);
        }
    }
    
    public static void LogAudit()
    {
        
    }

    public static void LogAccess()
    {
        
    }
}