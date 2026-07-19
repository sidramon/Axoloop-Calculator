namespace Domain.Calculator.Operations.Functions.Matrix;

using Domain.Calculator.Values;

public sealed class TransposeFunction : IFunction
{
    public string Name => "transpose";
    public int Arity => 1;
    public FunctionCategory Category => FunctionCategory.Matrix;
    public string Signature => "transpose(m)";

    public string Description =>
        "Transpose: swaps rows and columns. Returns a new matrix; m is never modified.";

    public IReadOnlyList<string> Examples => new[]
    {
        "transpose([1,2;3,4]) → [1,3;2,4]",
        "transpose([1,2,3]) → [1;2;3]",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        if (arguments[0] is not MatrixValue m)
            throw new InvalidOperationException("transpose requires a matrix.");
        return m.Transpose();
    }
}
