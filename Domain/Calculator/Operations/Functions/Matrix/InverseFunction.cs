namespace Domain.Calculator.Operations.Functions.Matrix;

using Domain.Calculator.Algorithms;
using Domain.Calculator.Values;

public sealed class InverseFunction : IFunction
{
    public string Name => "inverse";
    public int Arity => 1;
    public FunctionCategory Category => FunctionCategory.Matrix;
    public string Signature => "inverse(m)";

    public string Description =>
        "Inverse, computed by Gauss-Jordan elimination. Rejects singular matrices " +
        "(determinant zero, within a tolerance of 1e-12) by throwing, rather than " +
        "returning a matrix full of Infinity/NaN. Requires a square matrix.";

    public IReadOnlyList<string> Examples => new[]
    {
        "inverse([4,7;2,6]) → [0.6,-0.7;-0.2,0.4]",
        "inverse([1,2;2,4]) → Error: Matrix is singular and cannot be inverted.",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        if (arguments[0] is not MatrixValue m)
            throw new InvalidOperationException("inverse requires a matrix.");
        return GaussJordan.Invert(m);
    }
}
