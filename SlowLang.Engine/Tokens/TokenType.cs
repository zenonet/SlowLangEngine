namespace SlowLang.Engine.Tokens;

public enum TokenType
{
    String,
    Keyword,
    Int,
    Float,
    Bool,

    OpeningCurlyBracket,
    ClosingCurlyBracket,

    OpeningParenthesis,
    ClosingParenthesis,
    
    OpeningSquareBracket,
    ClosingSquareBracket,

    Equals,
    DoubleEquals,
    GreaterThan,
    LessThan,
    
    BooleanAnd,
    BooleanOr,
    
    Plus,
    Minus,
    Multiply,
    Divide,

    Semicolon,
    Comma,
    Colon,
    Dot,
    QuestionMark,
    ExclamationMark,
    
    SingleLineComment,
    MultiLineComment,
    ClosingMultiLineComment,
}