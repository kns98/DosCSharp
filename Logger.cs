
// Logger implementation
public class Logger : ILogger
{
    public void LogEvent(LogLevel level, string message)
    {
        Console.WriteLine($"[{level}] Event: {message}");
    }

    public void Log(LogLevel level, string message)
    {
        Console.WriteLine($"[{level}] {message}");
    }
}
