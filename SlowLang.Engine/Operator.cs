using SlowLang.Engine.Statements;
using SlowLang.Engine.Values;
using Zenonet.Utility;

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

    public override Value Execute()
    {
        Value? value = LeftOperand.Execute().ApplyOperator(
            new Subtype<Operator>(this.GetType()),
            RightOperand.Execute()
        );
        if(value == null)
            LoggingManager.LogError("Unable to apply operator " + this.GetType().Name + " to " + LeftOperand.GetType().Name + " and " + RightOperand.GetType().Name);
       
        return value!;
    }
}

public delegate Value OperatorDefinition(Value leftOperand, Value rightOperand);