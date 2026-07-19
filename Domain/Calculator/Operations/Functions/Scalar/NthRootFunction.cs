namespace Domain.Calculator.Operations.Functions.Scalar;

using Domain.Calculator.Values;

public sealed class NthRootFunction : IFunction
{
    public string Name => "nthroot";
    public int Arity => 2;
    public FunctionCategory Category => FunctionCategory.Arithmetic;
    public string Signature => "nthroot(x, n)";

    public string Description =>
        "Nth root of x. The radicand x comes first, the degree n second — the reverse of " +
        "how it's said out loud (\"cube root of 27\"). Accepts a negative radicand only if " +
        "n is an odd integer (e.g. nthroot(-8, 3) = -2); otherwise throws. n = 0 is always " +
        "invalid.";

    public IReadOnlyList<string> Examples => new[]
    {
        "nthroot(27, 3) → 3",
        "nthroot(-8, 3) → -2",
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
            if (!isOddInteger)
                throw new InvalidOperationException(
                    "nthroot of a negative number requires an odd integer degree.");

            return new NumberValue(-Math.Pow(-radicand, 1 / degree));
        }

        return new NumberValue(Math.Pow(radicand, 1 / degree));
    }
}
