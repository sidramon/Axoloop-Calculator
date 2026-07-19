namespace Domain.Calculator.Operations.Functions.Matrix;

using Domain.Calculator.Algorithms;
using Domain.Calculator.Values;

public sealed class RankFunction : IFunction
{
    public string Name => "rank";
    public int Arity => 1;
    public FunctionCategory Category => FunctionCategory.Matrix;
    public string Signature => "rank(m)";

    public string Description =>
        "Rank, computed by row echelon reduction with partial pivoting. A rank lower than " +
        "the smaller of the two dimensions indicates linearly dependent rows or columns. " +
        "Accepts rectangular matrices.";

    public IReadOnlyList<string> Examples => new[]
    {
        "rank([1,0;0,1]) → 2",
        "rank([1,2;2,4]) → 1",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        if (arguments[0] is not MatrixValue m)
            throw new InvalidOperationException("rank requires a matrix.");
        return new NumberValue(RowEchelon.Reduce(m).Rank);
    }
}
