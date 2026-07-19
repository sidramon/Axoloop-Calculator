namespace Domain.Calculator.Operations.Functions.Matrix;

using Domain.Calculator.Values;

public sealed class DotProductFunction : IFunction
{
    public string Name => "dotp";
    public int Arity => 2;
    public FunctionCategory Category => FunctionCategory.Matrix;
    public string Signature => "dotp(a, b)";

    public string Description =>
        "Dot product of two vectors (1×N or N×1 matrices). Requires vectors of equal " +
        "length; otherwise throws an exception stating both lengths involved.";

    public IReadOnlyList<string> Examples => new[]
    {
        "dotp([1,2,3],[4,5,6]) → 32",
        "dotp([1,2],[1,2,3]) → Error: Dot product requires vectors of equal length (2 vs 3).",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        if (arguments[0] is not MatrixValue a || arguments[1] is not MatrixValue b)
            throw new InvalidOperationException("dotp requires two vectors.");
        return new NumberValue(a.Dot(b));
    }
}
