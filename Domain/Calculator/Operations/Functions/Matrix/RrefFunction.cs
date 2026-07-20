namespace Domain.Calculator.Operations.Functions.Matrix;

using Domain.Calculator.Algorithms;
using Domain.Calculator.Values;

public sealed class RrefFunction : IFunction
{
    public string Name => "rref";
    public int Arity => 1;
    public FunctionCategory Category => FunctionCategory.Matrix;
    public string Signature => "rref(m)";

    public string Description =>
        "Reduced row echelon form, computed by Gauss-Jordan elimination: every pivot is " +
        "normalized to 1, and each pivot column is zero everywhere else. Useful to inspect a " +
        "system's structure directly, or to cross-check rank/linsolve results. Accepts " +
        "rectangular matrices.";

    public IReadOnlyList<string> Examples => new[]
    {
        "rref([1,2;2,4]) → [1,2;0,0]",
        "rref([2,4;1,3]) → [1,0;0,1]",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        var m = FunctionArguments.RequireMatrix(arguments[0], "rref");
        var (reduced, _) = RowEchelon.ReduceFully(m);
        return new MatrixValue(reduced);
    }
}
