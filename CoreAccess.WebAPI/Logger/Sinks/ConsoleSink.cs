using CoreAccess.WebAPI.Logger.Model;

namespace CoreAccess.WebAPI.Logger.Sinks;

public class ConsoleSink : ILogSink
{
    public void LogSystem(CoreLogLevel level,string source, string message, Exception? exception = null)
    {
        string output = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] [{source}] {message}";
        if (exception != null)
        {
            output += $"\nException: {exception.GetType().Name}: {exception.Message}\n{exception.StackTrace}";
        }
        
        Console.WriteLine(output);
    }
}