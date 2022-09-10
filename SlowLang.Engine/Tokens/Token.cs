namespace SlowLang.Engine.Tokens;

public class Token
{
    public readonly string RawContent;
    public readonly TokenType Type;
    public readonly int LineNumber;

    public Token(string rawContent, TokenType type, int lineNumber)
    {
        RawContent = rawContent;
        Type = type;
        LineNumber = lineNumber;
    }

    public override string ToString() => $"{Type}:{RawContent} in line {LineNumber}";
}