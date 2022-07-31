using SlowLang.Engine;
using SlowLang.Engine.Statements;
using SlowLang.Engine.Tokens;

const string path = @"..\..\..\first.helloWorldLang";

string code = File.ReadAllText(path);
TokenList tokenList = Lexer.Lex(code);

Statement[] statements = Statement.ParseMultiple(tokenList);
statements.Execute();