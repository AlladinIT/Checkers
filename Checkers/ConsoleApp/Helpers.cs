using Microsoft.Extensions.Logging;

namespace ConsoleApp;

public class Helpers
{
    public static readonly ILoggerFactory MyLoggerFactory =
        LoggerFactory.Create(builder =>
        {
            builder
                .AddFilter("Microsoft", LogLevel.Information)
                .AddFilter("System", LogLevel.Debug)
                .AddConsole();
        });
}