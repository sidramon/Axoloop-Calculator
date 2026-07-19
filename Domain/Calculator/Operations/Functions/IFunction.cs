namespace Domain.Calculator.Operations.Functions;

using Domain.Calculator.Values;

public interface IFunction
{
    string Name { get; }
    int Arity { get; }
    FunctionCategory Category { get; }
    string Signature { get; }
    string Description { get; }
    IReadOnlyList<string> Examples { get; }
    Value Apply(IReadOnlyList<Value> arguments);
}
