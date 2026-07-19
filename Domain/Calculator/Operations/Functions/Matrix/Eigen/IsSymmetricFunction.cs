using Domain.Calculator.Algorithms;

namespace Domain.Calculator.Operations.Functions.Matrix.Eigen;

using Domain.Calculator.Values;

public sealed class IsSymmetricFunction : IFunction
{
    public string Name => "issym";
    public int Arity => 1;
    public FunctionCategory Category => FunctionCategory.Eigen;
    public string Signature => "issym(m)";

    public string Description =>
        "Tests whether m is symmetric (m == transpose(m), within a tolerance of 1e-10). " +
        "A non-square matrix is never symmetric: returns False instead of throwing.";

    public IReadOnlyList<string> Examples => new[]
    {
        "issym([1,2;2,1]) → True",
        "issym([1,2;3,4]) → False",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        if (arguments[0] is not MatrixValue m)
            throw new InvalidOperationException("issym requires a matrix.");

        return new BooleanValue(EigenDecomposition.IsSymmetric(m));
    }
}
