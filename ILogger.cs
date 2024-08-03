
// Define a logger interface
interface ILogger
{
    void LogEvent(LogLevel level, string message);
    void Log(LogLevel level, string message);
}
