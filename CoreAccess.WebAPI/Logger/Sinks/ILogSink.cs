using CoreAccess.WebAPI.Logger.Model;

namespace CoreAccess.WebAPI.Logger.Sinks;

public interface ILogSink
{
    /// <summary>
    /// Logs a system message to the sink.
    /// </summary>
    /// <param name="level">The log level of the message.</param>
    /// <param name="source">The source of the log message, e.g., the class or module name.</param>
    /// <param name="message">The system message to log.</param>
    /// <param name="exception">An optional exception to log.</param>
    void LogSystem(CoreLogLevel level, string source, string message, Exception? exception = null);
}