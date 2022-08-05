using Microsoft.Extensions.Logging;
using SlowLang.Engine.Statements;

namespace SlowLang.Engine;

public static class LoggingManager
{
    public static TextWriter? OutputStream;
    public static TextReader? InputStream;

    /// <summary>
    /// The LoggerFactory used for all Loggers in the Project
    /// </summary>
    public static ILoggerFactory LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
    {
        builder
            .ClearProviders()
            .AddSimpleConsole()
            ;
    });

    public static readonly ILogger ErrorLogger = LoggerFactory.CreateLogger("SlowLang.Errors");

    public static void LogError(string errorMessage, Statement statement) => LogError(errorMessage, statement.LineNumber);

    public static void LogError(string errorMessage, int lineNumber)
    {
        ErrorLogger.LogError($"Error is line {lineNumber}: " + errorMessage);
        Environment.Exit(0);
    }

    public static void LogError(string errorMessage)
    {
        ErrorLogger.LogError("Error: " + errorMessage);
        Environment.Exit(0);
    }

    public static void SetLoggerFactory(ILoggerFactory loggerFactory)
    {
        LoggerFactory = loggerFactory;
    }
}