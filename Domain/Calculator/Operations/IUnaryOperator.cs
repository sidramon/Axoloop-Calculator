namespace Domain.Calculator.Operations;

using Domain.Calculator.Values;

public interface IUnaryOperator
{
    string Symbol { get; }
    Value Apply(Value operand);
}