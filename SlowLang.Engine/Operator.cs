using SlowLang.Engine;
using SlowLang.Engine.Statements;
using SlowLang.Engine.Values;

namespace SlowLang.Engine;

public abstract class Operator : StatementExtension
{
    public Statement LeftOperand = null!;
    public Statement RightOperand = null!;

    protected Operator(Statement leftOperand, Statement rightOperand)
    {
        LeftOperand = leftOperand;
        RightOperand = rightOperand;
    }

    protected Operator()
    {
        
    }
}
public delegate Value OperatorDefinition(Value leftOperand, Value rightOperand);