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
TODO

