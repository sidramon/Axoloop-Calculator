namespace Domain.Calculator.Operations.SpecialForms;

using Domain.Calculator.Ast;
using Domain.Calculator.Operations.Functions;
using Domain.Calculator.Values;

public sealed class IfForm : ISpecialForm
{
    public string Name => "if";
    public int Arity => 3;
    public FunctionCategory Category => FunctionCategory.Logic;
    public string Signature => "if(condition, then, else)";

    public string Description =>
        "Lazy conditional branch: evaluates only the chosen branch, unlike an ordinary " +
        "function whose arguments are all evaluated before the call. Essential for " +
        "recursive functions with a base case — otherwise the recursive branch would " +
        "always be evaluated and loop forever. condition must evaluate to a boolean " +
        "(comparison, and/or/not), otherwise throws.";

    public IReadOnlyList<string> Examples => new[]
    {
        "if(1 < 2, 10, 20) → 10",
        "fact(n) := if(n <= 1, 1, n * fact(n-1))",
    };

    public Value Apply(IReadOnlyList<IExpression> arguments, VariableContext context, Evaluator evaluator)
    {
        var condition = evaluator.Evaluate(arguments[0], context);
        if (condition is not BooleanValue boolean)
            throw new InvalidOperationException("condition must be a boolean.");

        return boolean.Boolean
            ? evaluator.Evaluate(arguments[1], context)
            : evaluator.Evaluate(arguments[2], context);
    }
}
