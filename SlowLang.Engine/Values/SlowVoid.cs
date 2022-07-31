using SlowLang.Engine.Tokens;

namespace SlowLang.Engine.Values;

/// <summary>
/// Used as the return type of void functions
/// </summary>
public class SlowVoid : Value
{
    public static readonly SlowVoid I = new ();
    
    public static bool TryParse(ref TokenList tokenList, out Value? value)
    {
        if (tokenList.Peek().RawContent == "void")
        {
            tokenList.Pop();
            value = I;
            return true;
        }

        value = null;
        return false;
    }
}