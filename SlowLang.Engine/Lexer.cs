﻿using System.Text.RegularExpressions;
using SlowLang.Engine.Tokens;

namespace SlowLang.Engine;

/// <summary>
/// A system to convert a string into TokenList which are easier to parse and more efficient 
/// </summary>
public static class Lexer
{
    private static readonly string NewLine = Environment.NewLine;

    private static Dictionary<string, TokenType> tokenDefinitions = new();
    
    private static readonly Dictionary<string, TokenType> DefaultTokenDefinitions = new()
    {
        {"\".*?\"", TokenType.String},

        {@"\(", TokenType.OpeningParenthesis},
        {@"\)", TokenType.ClosingParenthesis},

        {@"\{", TokenType.OpeningCurlyBracket},
        {@"\}", TokenType.ClosingCurlyBracket},
        
        {@"\[", TokenType.OpeningSquareBracket},
        {@"\]", TokenType.ClosingSquareBracket},

        {@"==", TokenType.DoubleEquals},
        {@"!", TokenType.ExclamationMark},
        {@"&&", TokenType.BooleanAnd},
        {@"\|\|", TokenType.BooleanOr},

        {@"(?:\d+\.?\d*(?:f|F)|\d*\.\d+(?:f|F)?)", TokenType.Float},
        {@"\d+", TokenType.Int},
        {@"(?:(?:t|T)(?:rue|RUE)|(?:f|F)(?:alse|ALSE))", TokenType.Bool},

        {@";", TokenType.Semicolon},
        {@",", TokenType.Comma},
        {@":", TokenType.Colon},
        {@"\.", TokenType.Dot},
        {@"\?", TokenType.QuestionMark},
        
        {@"\s*=\s*", TokenType.Equals},
        {@"\s*<\s*", TokenType.LessThan},
        {@"\s*>\s*", TokenType.GreaterThan},
        
        {@"\s*\+\s*", TokenType.Plus},
        {@"\s*-\s*", TokenType.Minus},
        {@"\s*\*\s*", TokenType.Multiply},
        {@"\s*/\s*", TokenType.Divide},
        
        {@"\s//\s", TokenType.SingleLineComment},
        {@"\s*\/\*\s*", TokenType.MultiLineComment},
        {@"\s*\*\/\s*", TokenType.ClosingMultiLineComment},

        {@"\w+", TokenType.Keyword}, //Needs to be the last one because it would accept nearly anything
    };

    /// <summary>
    /// Adds a token to the TokenDefinitions
    /// </summary>
    /// <param name="regexPattern">The pattern to search for</param>
    /// <param name="tokenType">The token to reference it with</param>
    public static void DefineToken(string regexPattern, TokenType tokenType)
    {
        tokenDefinitions.Add(regexPattern, tokenType);
    }

    /// <summary>
    /// Sets all TokenDefinitions to a dictionary
    /// </summary>
    /// <param name="definitions">A Dictionary where the keys are regex patterns and the values are the TokenTypes</param>
    public static void DefineTokens(Dictionary<string, TokenType> definitions)
    {
        tokenDefinitions = definitions;
    }

    public static TokenList Lex(string code)
    {
        //If no Tokens are defined, fall back to the default tokens
        if (tokenDefinitions.Count < 1)
            tokenDefinitions = DefaultTokenDefinitions;

        TokenList tokenList = new TokenList();
        int lineNumber = 1;
        while (code.Length > 0)
        {
            code = code.TrimStart(' ');
            while (code.StartsWith(NewLine))
            {
                code = code[NewLine.Length..];
                lineNumber++;

                code = code.TrimStart(' ');
            }

            //Iterate through all defined tokens
            foreach (KeyValuePair<string, TokenType> tokenDefinition in tokenDefinitions)
            {
                //Try to match them at the start of code
                Match match = Regex.Match(code, "^" + tokenDefinition.Key);

                if (!match.Success)
                    continue;

                //If the current tokenDefinition got matched successfully, add it to the TokenList
                tokenList.Add(new Token(match.Value, tokenDefinition.Value, lineNumber));

                //Remove the matched region from code
                code = code[match.Value.Length..];

                //And break out of the foreach loop
                break;
            }
        }

        return tokenList;
    }
}