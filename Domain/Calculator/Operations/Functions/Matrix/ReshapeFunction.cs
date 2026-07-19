namespace Domain.Calculator.Operations.Functions.Matrix;

using Domain.Calculator.Values;

public sealed class ReshapeFunction : IFunction
{
    public string Name => "reshape";
    public int Arity => 3;
    public FunctionCategory Category => FunctionCategory.Matrix;
    public string Signature => "reshape(m, rows, cols)";

    public string Description =>
        "Rearranges the elements of m into a rows×cols matrix, walking in ROW-MAJOR order " +
        "(NumPy convention: row by row), not column-major (MATLAB convention). The total " +
        "element count must match exactly (rows * cols = m.Rows * m.Columns), otherwise " +
        "throws.";

    public IReadOnlyList<string> Examples => new[]
    {
        "reshape([1,2,3,4,5,6],2,3) → [1,2,3;4,5,6]",
        "reshape([1,2,3],2,2) → Error: Cannot reshape 1x3 (3 elements) into 2x2 (4 elements).",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        var matrix = FunctionArguments.RequireMatrix(arguments[0], "reshape");
        var rows = FunctionArguments.RequireSize(arguments[1], "reshape");
        var columns = FunctionArguments.RequireSize(arguments[2], "reshape");
        return matrix.Reshape(rows, columns);
    }
}
