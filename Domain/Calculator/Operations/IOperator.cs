namespace Domain.Calculator.Operations;

using Domain.Calculator.Values;

public interface IOperator
{
    string Symbol { get; }
    int Precedence { get; }
    Associativity Associativity { get; }
    Value Apply(Value left, Value right);
}