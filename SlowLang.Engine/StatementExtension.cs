using SlowLang.Engine.Statements;
using SlowLang.Engine.Tokens;

namespace SlowLang.Engine;

public abstract class StatementExtension : Statement
{
    protected override bool OnParse(ref TokenList list)
    {
        return true;
    }

    public virtual void OnParse(ref TokenList list, Statement baseStatement)
    {
    }
}