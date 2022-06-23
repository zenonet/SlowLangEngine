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
    /// <returns></returns>
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
    /// <param name="input">The input value</param>
    /// <param name="targetType">The type to convert the input into</param>
    /// <param name="output">The Value that got converted (hopefully)</param>
    /// <returns>A bool that determines whether the conversion was successful</returns>
    public virtual bool TryConvertImplicitly(Value input, Type targetType, [MaybeNullWhen(false)] out Value output)
    {
        if (input.GetType() == targetType)
        {
            output = input;
            return true;
        }

        output = null;
        return false;
    }
    
    /// <summary>
    /// Converts a value into another Value implicitly.
    /// Throws an error if the conversion doesn't work
    /// </summary>
    /// <param name="input">The input value</param>
    /// <param name="targetType">The type to convert the input into</param>
    /// <returns>The value that got converted (hopefully)</returns>
    public virtual Value ConvertImplicitly(Value input, Type targetType)
    {
        if (input.GetType() == targetType)
        {
            return input;
        }
        LoggingManager.LogError($"Unable to implicitly covert {input} to {targetType.Name}");
        return null!;
    }

    /// <summary>
    /// Indicates whether this value object has a value
    /// </summary>
    public bool IsVoid => this == SlowVoid.I;
}