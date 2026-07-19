namespace Domain.Calculator.Operations.Functions.Matrix;

using Domain.Calculator.Values;

public sealed class CrossProductFunction : IFunction
{
    public string Name => "crossp";
    public int Arity => 2;
    public FunctionCategory Category => FunctionCategory.Matrix;
    public string Signature => "crossp(a, b)";

    public string Description =>
        "Cross product of two 3-element vectors. Preserves the orientation (row or column) " +
        "of the LEFT operand, not the second argument's: crossp(row, col) returns a row " +
        "vector. Requires two vectors with exactly 3 elements.";

    public IReadOnlyList<string> Examples => new[]
    {
        "crossp([1,0,0],[0,1,0]) → [0,0,1]",
        "crossp([1;0;0],[0,1,0]) → [0;0;1]",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        if (arguments[0] is not MatrixValue a || arguments[1] is not MatrixValue b)
            throw new InvalidOperationException("crossp requires two vectors.");
        return a.Cross(b);
    }
}
