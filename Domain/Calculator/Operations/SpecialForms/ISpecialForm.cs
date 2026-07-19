namespace Domain.Calculator.Operations.SpecialForms;

using Domain.Calculator.Ast;
using Domain.Calculator.Operations.Functions;
using Domain.Calculator.Values;

public interface ISpecialForm
{
    string Name { get; }
    int Arity { get; }
    FunctionCategory Category { get; }
    string Signature { get; }
    string Description { get; }
    IReadOnlyList<string> Examples { get; }
    Value Apply(IReadOnlyList<IExpression> arguments, VariableContext context, Evaluator evaluator);
}
