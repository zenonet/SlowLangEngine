using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Extensions.Logging;
using SlowLang.Engine.Tokens;

namespace SlowLang.Engine.Values;

/// <summary>
/// Represents a single Value in a SlowLang script
/// </summary>
public abstract class Value
{
    private static readonly ILogger Logger = LoggingManager.LoggerFactory.CreateLogger("SlowLang.ValueSystem");


    /// <summary>
    /// Contains all variables in the current script
    /// </summary>
    public static readonly Dictionary<string, Value> Variables = new();


    /// <summary>
    /// Parses a TokenList into a Value object
    /// </summary>
    /// <returns>The parsed value object</returns>
    public static Value? Parse(TokenList tokenList)
    {
        //Iterate through all inheritors of Value
        foreach (Type inheritor in ParsingUtility.GetAllInheritors(typeof(Value)))
        {
            //Get their TryParse method
            MethodInfo? method = inheritor.GetMethod("TryParse");

            //If the method doesn't exist, just skip it
            if (method is null)
                continue;

            //Create the parameters array
            object?[] parameters = {tokenList, null};

            //Invoke the method
            bool worked = (bool) method.Invoke(null, parameters)!;

            //If the Parser wasn't able to Parse the value, continue with the next inheritor
            if (!worked)
                continue;

            //If the parsing was successful return the Value that got parsed
            if (parameters[1] is Value)
                return (Value) parameters[1]!;

            //If not, throw a warning
            Logger.LogWarning(
                "{name} implements an invalid definition of TryParse()",
                inheritor.Name);
        }

        //If none of the value parsers was successful, log an error
        Logger.LogError("Unable to Parse {tokenList}", tokenList);


        //And return null
        return null;
    }

    /// <summary>
    /// Tries to convert a value into another Value implicitly
    /// </summary>
    /// <param name="output">The Value that got converted (hopefully)</param>
    /// <returns>A bool that determines whether the conversion was successful</returns>
    public bool TryConvertImplicitly<TTarget>([MaybeNullWhen(false)] out Value output) where TTarget : Value
    {
        return TryConvertImplicitly(typeof(TTarget), out output);
    }

    /// <summary>
    /// Tries to convert a value into another Value implicitly
    /// </summary>
    /// <param name="targetType">The type to convert the input into</param>
    /// <param name="output">The Value that got converted (hopefully)</param>
    /// <returns>A bool that determines whether the conversion was successful</returns>
    public virtual bool TryConvertImplicitly(Type targetType, [MaybeNullWhen(false)] out Value output)
    {
        if (this.GetType() == targetType)
        {
            output = this;
            return true;
        }

        output = null;
        return false;
    }

    /// <summary>
    /// Converts a value into another Value implicitly.
    /// Throws an error if the conversion doesn't work
    /// </summary>
    /// <returns>The value that got converted (hopefully)</returns>
    public TTarget ConvertImplicitly<TTarget>() where TTarget : Value
    {
        if (TryConvertImplicitly(typeof(TTarget), out Value convertedValue))
        {
            return (TTarget) convertedValue;
        }

        LoggingManager.LogError($"Unable to implicitly covert {this} to {typeof(TTarget).Name}");
        return null!;
    }

    /// <summary>
    /// Converts a value into another Value implicitly.
    /// Throws an error if the conversion doesn't work
    /// </summary>
    /// <param name="targetType">The type to convert the input into</param>
    /// <returns>The value that got converted (hopefully)</returns>
    public Value ConvertImplicitly(Type targetType)
    {
        if (TryConvertImplicitly(targetType, out Value convertedValue))
        {
            return convertedValue;
        }

        LoggingManager.LogError($"Unable to implicitly covert {this} to {targetType.Name}");
        return null!;
    }

    public static Type? ParseTypeKeyword(Token token)
    {
        if (token.Type != TokenType.Keyword)
            return null;
        
        //Uhm...how should I explain this?
        //Basically I created a foreach loop with all the logic and my IDE said: "Hey, I can make this into a LINQ statement for you"
        return (from valueType in ParsingUtility.GetAllInheritors(typeof(Value))
            let methodInfo = valueType.GetMethod("GetKeyword")
            where methodInfo is not null &&
                  methodInfo.ReturnType == typeof(string) &&
                  methodInfo.GetParameters() == Array.Empty<ParameterInfo>()
            let keyword = (string) methodInfo.Invoke(null,
                null)!
            where keyword == token.RawContent
            select valueType).FirstOrDefault();
    }

    /// <summary>
    /// Indicates whether this value object has a value
    /// </summary>
    public bool IsVoid => this == SlowVoid.I;
}