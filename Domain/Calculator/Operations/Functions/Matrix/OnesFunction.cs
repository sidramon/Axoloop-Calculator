namespace Domain.Calculator.Operations.Functions.Matrix;

using Domain.Calculator.Values;

public sealed class OnesFunction : IFunction
{
    public string Name => "ones";
    public int Arity => 2;
    public FunctionCategory Category => FunctionCategory.Matrix;
    public string Signature => "ones(rows, cols)";

    public string Description =>
        "rows×cols matrix filled with ones. rows and cols must be integers greater than " +
        "or equal to 1.";

    public IReadOnlyList<string> Examples => new[]
    {
        "ones(2,2) → [1,1;1,1]",
        "ones(1,3) → [1,1,1]",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        var rows = FunctionArguments.RequireSize(arguments[0], "ones");
        var columns = FunctionArguments.RequireSize(arguments[1], "ones");
        return MatrixValue.Filled(rows, columns, 1);
    }
}
