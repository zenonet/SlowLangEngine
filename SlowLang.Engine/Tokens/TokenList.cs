using System.Collections;
using System.Diagnostics.Contracts;

namespace SlowLang.Engine.Tokens;

public class TokenList : IEnumerable<Token>
{
    public readonly List<Token> List = new();

    /// <summary>
    /// Adds a Token to the top of the list
    /// </summary>
    /// <param name="token">The token to add</param>
    public void Add(Token token) => List.Add(token);

    /// <summary>
    /// Gets the first token in the list but doesn't remove it
    /// </summary>
    /// <returns>The first token</returns>
    /// <param name="offset">Offsets the index to peek at</param>
    [Pure]
    public Token Peek(int offset = 0)
    {
        if (List.Count > offset)
            return List[offset];

        LoggingManager.LogError("Invalid syntax at end of file");
        return null!;
    }


    /// <summary>
    /// Checks if the token at a specific position is of a specific TokenType
    /// </summary>
    /// <param name="expectedType">The TokenType that is expected</param>
    /// <param name="offset">Offsets the index to look at</param>
    [Pure]
    public bool StartsWith(TokenType expectedType, int offset = 0)
    {
        if (List.Count > offset)
            return List[0].Type == expectedType;

        return false;
    }

    #region TrimMethods

    /// <summary>
    /// Trims all occurrences of TokenTypes from the start of the TokenList
    /// WARNING: This method wasn't tested yet (because i was too lazy)
    /// </summary>
    /// <param name="typesToTrim">An array of all TokenType that should get cut</param>
    public void TrimStart(params TokenType[] typesToTrim)
    {
        while (typesToTrim.Contains(List[0].Type))
        {
            foreach (TokenType type in typesToTrim)
            {
                if (List.Count < 1)
                    return;

                if (List[0].Type != type)
                    continue;

                List.RemoveAt(0);
            }
        }
    }
    /// <summary>
    /// Trims all occurrences of a TokenType from the start of the TokenList
    /// WARNING: This method wasn't tested yet (because i was too lazy)
    /// </summary>
    /// <param name="typeToTrim">The TokenType that should get cut</param>
    public void TrimStart(TokenType typeToTrim)
    {
        //I created these TrimStart and TrimEnd overloads with just a single TokenType because this won't allocate an array just for one TokenType

        if(List.Count < 1)
            return;
        
        while (List.Count > 0 && List[0].Type == typeToTrim)
        {
            List.RemoveAt(0);
        }
    }


    /// <summary>
    /// Trims all occurrences of TokenTypes from the end of the TokenList
    /// WARNING: This method wasn't tested yet (because i was too lazy)
    /// </summary>
    /// <param name="typesToTrim">An array of all TokenType that should get cut</param>
    public void TrimEnd(params TokenType[] typesToTrim)
    {

        while (typesToTrim.Contains(List[0].Type))
        {
            foreach (TokenType type in typesToTrim)
            {
                if (List.Count < 1)
                    return;

                if (List[^1].Type != type)
                    continue;

                List.RemoveAt(List.Count - 1);
                
            }
        }
    }
    
    /// <summary>
    /// Trims all occurrences of a TokenType from the end of the TokenList
    /// WARNING: This method wasn't tested yet (because i was too lazy)
    /// </summary>
    /// <param name="typeToTrim">The TokenType that should get cut</param>
    public void TrimEnd(TokenType typeToTrim)
    {
        //I created these TrimStart and TrimEnd overloads with just a single TokenType because this won't allocate an array just for one TokenType

        if(List.Count < 1)
            return;

        while (List.Count > 0 && List[^0].Type == typeToTrim)
        {
            List.RemoveAt(List.Count - 1);
        }
    }

    #endregion

    /// <summary>
    /// Gets the first token and removes it
    /// </summary>
    /// <returns>Offsets the index to get at (Not recommended)</returns>
    public Token Pop(int offset = 0)
    {
        if (List.Count < offset)
            return null!;

        Token first = List[offset];
        List.RemoveAt(offset);
        return first;
    }


    /// <summary>
    /// Splits a TokenList at a separator
    /// </summary>
    /// <param name="separator">The separator to split at</param>
    /// <returns>An array of TokenLists</returns>
    public TokenList[] Split(TokenType separator)
    {
        List<TokenList> tokenLists = new();

        if (List.Count > 0)
            tokenLists.Add(new TokenList());

        foreach (Token token in List)
        {
            if (token.Type == separator)
            {
                tokenLists.Add(new TokenList());
                continue;
            }

            tokenLists.Last().Add(token);
        }

        return tokenLists.ToArray();
    }

    /// <summary>
    /// Removes a range of elements from the TokenList
    /// </summary>
    /// <param name="range">The range to remove</param>
    public void RemoveRange(Range range)
    {
        if (List.Count < range.End.Value)
            List.Clear();
        else
            List.RemoveRange(range.Start.Value, range.End.Value - range.Start.Value);
    }


    #region Contracts

    public IEnumerator<Token> GetEnumerator() => List.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString()
    {
        string output = "";
        List.ForEach(x => output += "\n" + x);
        //Trim the line break at the Start and return
        return output.TrimStart('\n');
    }

    public static implicit operator List<Token>(TokenList tokenList) => tokenList.List;


    public Token this[int index] => List[index];


    internal TokenList(IEnumerable<Token> tokens)
    {
        List = tokens.ToList();
    }

    public TokenList()
    {
    }

    #endregion
}