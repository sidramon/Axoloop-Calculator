namespace Domain.Calculator.Operations.Functions.Matrix;

using Domain.Calculator.Values;

public sealed class TraceFunction : IFunction
{
    public string Name => "trace";
    public int Arity => 1;
    public FunctionCategory Category => FunctionCategory.Matrix;
    public string Signature => "trace(m)";

    public string Description =>
        "Trace: sum of the elements on the main diagonal. Requires a square matrix.";

    public IReadOnlyList<string> Examples => new[]
    {
        "trace([1,2;3,4]) → 5",
        "trace([1,2,3;4,5,6]) → Error: Trace requires a square matrix.",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        if (arguments[0] is not MatrixValue m)
            throw new InvalidOperationException("trace requires a matrix.");
        return new NumberValue(m.Trace());
    }
}
