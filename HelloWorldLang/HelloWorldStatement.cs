using SlowLang.Engine;
using SlowLang.Engine.Statements;
using SlowLang.Engine.Statements.StatementRegistrations;
using SlowLang.Engine.Tokens;
using SlowLang.Engine.Values;

namespace HelloWorldLang;

public class HelloWorldStatement : Statement
{
    public static void OnInitialize()
    {
        StatementRegistration.Create<HelloWorldStatement>(
                TokenType.Keyword,
                TokenType.OpeningBrace,
                TokenType.String,
                TokenType.ClosingBrace,
                TokenType.Semicolon
            )
            .Register();
    }

    private string parameter = null!;
    
    protected override bool OnParse(ref TokenList list)
    {
        string keyword = list.Pop().RawContent; // Cut the keyword
        
        //Check if the keyword really is helloworld
        //If not, throw an error
        if(keyword.ToLower() != "helloworld")
            LoggingManager.LogError($"Unable to parse {keyword}", LineNumber);
        
        list.Pop(); // Cut the opening brace

        //Get the token between the braces
        parameter = list.Pop().RawContent;

        //Because we didn't override CutTokensManually(), it defaults to false meaning we
        //don't have to worry about completely removing the statement from the TokenList
        //The parser will just remove the parsed TokenType sequence for us

        //Tell the parser that we successfully parsed the statement
        return true;
    }

    public override Value Execute()
    {
        
        Console.WriteLine($"Hello {parameter}");
        
        //The value to return if a Statement doesn't have a return value
        return SlowVoid.I;
    }
}