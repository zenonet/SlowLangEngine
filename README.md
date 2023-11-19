![Nuget](https://img.shields.io/nuget/v/SlowLang.Engine)
![Nuget](https://img.shields.io/nuget/dt/SlowLang.Engine)
![Lines of code](https://img.shields.io/tokei/lines/github/zenonet/SlowLangEngine)

# SlowLangEngine

SlowLangEngine is a C# library for easily creating simple programming languages. It was originally part of [SlowLang](https://github.com/zenonet/SlowLang/) but eventually became an indipendent project

## Features

- A simple lexer
- A very extensible parser
- Simple error handling
- A simple but extensible value system

## Quick start

* Create a new C# project.
* Import SlowLang.Engine from nuget <br>
  using your IDE or using the dotnet CLI like this: `dotnet add package SlowLang.Engine`
* Copy this code into your Program.cs
```c#
using SlowLang.Engine;
using SlowLang.Engine.Statements;
using SlowLang.Engine.Tokens;

const string path = @"<YourSourceFile>";

string code = File.ReadAllText(path);
TokenList tokenList = Lexer.Lex(code);

Statement[] statements = Statement.ParseMultiple(tokenList);

//This is an extension method on IEnumerable<Statement>
//that will execute all Statements in the list.
statements.Execute();
```

This will lex, parse and execute your code

* Now you can create your own Statements like this:
```C#
public class MyStatement : Statement, IInitializable
{
    private string keyword;

    //This static method will automatically get called on all classes that implement IInitializable
    public static void Initialize()
    {
        //Here, you can register you Statement so you don't have to do that at a central location
        
        StatementRegistration.Create<MyStatement>(
                //This is a sequence of Tokens that need to be matched in order to start the parsing process.
                TokenType.Keyword,
                TokenType.OpeningBrace,
                TokenType.ClosingBrace,
                TokenType.Semicolon
            )
            .Register();
    }
    
    
    protected override void OnParse(ref TokenList list)
    {
        //On parse gets called as soon as the parsing process begins
    
        keyword = list.Peek().RawContent; // Get the keyword from the Tokenlist and store its value
        
        //The parser will automatically remove the rest of the Statements tokens from the TokenList
    }
    
    public override Value Execute()
    {
        //This method gets called every time this Statement gets executed
        
        Console.WriteLine($"Function {keyword} was called");
        
        //Return the return value of the Statement
        //Since this Statement doesn't have a return value, just return SlowVoid.I (NOT NULL!)
        return SlowVoid.I;
    }

}
```
* Create your source code file and save for example
```hwl
hello();
```
This should now write "Function hello was called" to the console

## Created with SlowLangEngine

* [SlowLang](https://github.com/zenonet/SlowLang/)
* [McFuncScript](https://github.com/zenonet/McFuncSharp/#mcfuncscript)

