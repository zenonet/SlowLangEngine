using SlowLang.Engine.Statements;
using SlowLang.Engine.Tokens;

namespace SlowLang.Engine;

public abstract class StatementExtension : Statement
{
    protected override void OnParse(ref TokenList list)
    {
        
    }

    public virtual void OnParse(ref TokenList list, Statement baseStatement)
    {
        
    }
}