namespace Domain.Calculator.Operations.Functions.Matrix;

using Domain.Calculator.Values;

public sealed class ZerosFunction : IFunction
{
    public string Name => "zeros";
    public int Arity => 2;
    public FunctionCategory Category => FunctionCategory.Matrix;
    public string Signature => "zeros(rows, cols)";

    public string Description =>
        "rows×cols matrix filled with zeros. rows and cols must be integers greater than " +
        "or equal to 1.";

    public IReadOnlyList<string> Examples => new[]
    {
        "zeros(2,3) → [0,0,0;0,0,0]",
        "zeros(0,2) → Error: Matrix dimensions must be at least 1x1.",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        var rows = FunctionArguments.RequireSize(arguments[0], "zeros");
        var columns = FunctionArguments.RequireSize(arguments[1], "zeros");
        return MatrixValue.Filled(rows, columns, 0);
    }
}
