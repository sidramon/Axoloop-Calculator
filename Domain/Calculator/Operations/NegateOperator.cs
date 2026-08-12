namespace Domain.Calculator.Operations;

using Domain.Calculator.Values;

public sealed class NegateOperator : IUnaryOperator
{
    public string Symbol => "-";

    public Value Apply(Value operand) => operand switch
    {
        NumberValue n => new NumberValue(-n.Number),
        ComplexValue c => ComplexValue.Of(-c.Real, -c.Imaginary),
        MatrixValue m => m.Scale(-1),
        _ => throw new InvalidOperationException("Negation requires a number or a matrix.")
    };
}