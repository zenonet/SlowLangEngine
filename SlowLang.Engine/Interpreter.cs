using Microsoft.Extensions.Logging;
using SlowLang.Engine.Statements;
using SlowLang.Engine.Tokens;

namespace SlowLang.Engine;

public static class Interpreter
{
    public static TextWriter? OutputStream;
    public static TextReader? InputStream;
    
    
    
    
    /// <summary>
    /// The LoggerFactory used for all Loggers in the Project
    /// </summary>
    public static readonly ILoggerFactory LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
    {
        builder
            .ClearProviders()
            .AddSimpleConsole()
            ;
    });

    internal static readonly ILogger Logger = LoggerFactory.CreateLogger("SlowLang.Engine");

    internal static readonly ILogger ErrorLogger = LoggerFactory.CreateLogger("SlowLang.Errors");


    internal static void LogError(string errorMessage, Statement statement) => LogError(errorMessage, statement.LineNumber);

    internal static void LogError(string errorMessage, int lineNumber)
    {
        ErrorLogger.LogError($"Error is line {lineNumber}: " + errorMessage);
        Environment.Exit(0);
    }
    internal static void LogError(string errorMessage)
    {
        ErrorLogger.LogError("Error: " + errorMessage);
        Environment.Exit(0);
    }

    public static void RunScript(string code)
    {
        OutputStream ??= Console.Out;
        InputStream ??= Console.In;
        
        TokenList tokenList = Lexer.Lex(code);
        
        Statement[] statements = Statement.ParseMultiple(tokenList);
        statements.Execute();
    }
}