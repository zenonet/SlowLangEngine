using SlowLang.Engine.Statements;
using SlowLang.Engine.Tokens;

namespace SlowLang.Engine;

public abstract class StatementExtension : Statement
{
    public override bool OnParse(ref TokenList list)
    {
        return true;
    }

    public virtual bool OnParse(ref TokenList list, Statement baseStatement)
    {
        return true;
    }
}