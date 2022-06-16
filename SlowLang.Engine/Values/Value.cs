using System.Reflection;
using Microsoft.Extensions.Logging;
using SlowLang.Engine.Tokens;

namespace SlowLang.Engine.Values;

/// <summary>
/// Represents a single Value in a SlowLang script
/// </summary>
public abstract class Value
{
    private static readonly ILogger Logger = Interpreter.LoggerFactory.CreateLogger("SlowLang.ValueSystem");

    
    

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
            if(method is null)
                continue;
            
            //Create the parameters array
            object?[] parameters = {tokenList, null };
            
            //Invoke the method
            bool worked = (bool) method.Invoke(null, parameters )!;
            
            //If the Parser wasn't able to Parse the value, continue with the next inheritor
            if (!worked) 
                continue;
            
            //If the parsing was successful return the Value that got parsed
            if (parameters[1] is Value) 
                return (Value)parameters[1]!;
                
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

    //TODO: find a better name since Value.HasValue doesn't really make sense
    public bool HasValue => this != SlowVoid.I;
}