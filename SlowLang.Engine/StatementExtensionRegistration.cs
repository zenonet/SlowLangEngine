using SlowLang.Engine.Tokens;

namespace SlowLang.Engine.Statements.StatementRegistrations;

public readonly struct StatementExtensionRegistration
{
    public readonly Type BaseStatement;
    public readonly Type ExtensionStatement;
    public readonly CustomParser? CustomParser;
    public readonly TokenType[] Match;

    private StatementExtensionRegistration(Type extensionStatement, Type baseStatement, CustomParser? customParser, params TokenType[] match)
    {
        Match = match;
        CustomParser = customParser;
        ExtensionStatement = extensionStatement;
        BaseStatement = baseStatement;
    }
    
    
    public void Register()
    {
        Statement.RegisterExtension(this);
    }

    #region Create methods

    public static StatementExtensionRegistration CreateStatementExtensionRegistration<TBase, TExtension>(
        CustomParser customParser,
        params TokenType[] match)
        where TBase : Statement
        where TExtension : Statement
    {
        return new StatementExtensionRegistration(typeof(TExtension), typeof(TBase), customParser);
    }

    public static StatementExtensionRegistration CreateStatementExtensionRegistration<TBase, TExtension>(
        params TokenType[] match)
        where TBase : Statement
        where TExtension : Statement
    {
        return new StatementExtensionRegistration(typeof(TExtension), typeof(TBase), null);
    }

    #endregion
}