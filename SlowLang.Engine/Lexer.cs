using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using SlowLang.Engine.Tokens;

namespace SlowLang.Engine;

/// <summary>
/// A system to convert a string into TokenList which are easier to parse and more efficient 
/// </summary>
public static class Lexer
{
    private static readonly ILogger Logger = LoggingManager.LoggerFactory.CreateLogger("SlowLang.Lexer");

    private static readonly string NewLine = Environment.NewLine;

    private static Dictionary<string, TokenType> tokenDefinitions = new();

    public static void DefineToken(string regexPattern, TokenType tokenType)
    {
        tokenDefinitions.Add(regexPattern, tokenType);
    }

    public static void DefineTokens(Dictionary<string, TokenType> definitions)
    {
        tokenDefinitions = definitions;
    }

    public static TokenList Lex(string code)
    {
        TokenList tokenList = new TokenList();
        int lineNumber = 1;
        while (code.Length > 0)
        {
            while (code.StartsWith(NewLine))
            {
                code = code[NewLine.Length..];
                lineNumber++;
            }

            //Iterate through all defined tokens
            foreach (KeyValuePair<string,TokenType> tokenDefinition in tokenDefinitions)
            {
                //Try to match them at the start of code
                Match match = Regex.Match(code, "^" + tokenDefinition.Key);
                
                if(!match.Success)
                    continue;
                
                //If the current tokenDefinition got matched successfully, add it to the TokenList
                tokenList.Add(new Token(match.Value, tokenDefinition.Value, lineNumber));
                
                //Remove the matched region from code
                code = code.Substring(match.Value.Length);

                //And break out of the foreach loop
                break;
            }
        }

        Logger.LogInformation("Lexed:" + tokenList);
        return tokenList;
    }
}