namespace Domain.Calculator.Operations.Functions.Scalar;

using System.Numerics;
using Domain.Calculator.Values;

public sealed class NthRootFunction : IFunction
{
    public string Name => "nthroot";
    public int Arity => 2;
    public FunctionCategory Category => FunctionCategory.Arithmetic;
    public string Signature => "nthroot(x, n)";

    public string Description =>
        "Nth root of x. The radicand x comes first, the degree n second — the reverse of " +
        "how it's said out loud (\"cube root of 27\"). A negative radicand with an odd " +
        "integer degree still returns the real root (e.g. nthroot(-8, 3) = -2); any other " +
        "negative-radicand case (even degree, or a non-integer degree) now returns the " +
        "principal complex root instead of throwing. n = 0 is always invalid.";

    public IReadOnlyList<string> Examples => new[]
    {
        "nthroot(27, 3) → 3",
        "nthroot(-8, 3) → -2",
        "nthroot(-8, 2) → 2.8284i",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        if (arguments[0] is not NumberValue x || arguments[1] is not NumberValue n)
            throw new InvalidOperationException("nthroot requires two numbers.");

        var degree = n.Number;
        var radicand = x.Number;

        if (degree == 0)
            throw new InvalidOperationException("nthroot degree cannot be zero.");

        if (radicand < 0)
        {
            var isOddInteger = degree % 1 == 0 && Math.Abs(degree % 2) == 1;
            if (isOddInteger)
                return new NumberValue(-Math.Pow(-radicand, 1 / degree));

            return ComplexValue.Of(Complex.Pow(new Complex(radicand, 0), 1 / degree));
        }

        return new NumberValue(Math.Pow(radicand, 1 / degree));
    }
}
