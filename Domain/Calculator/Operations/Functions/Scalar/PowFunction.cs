namespace Domain.Calculator.Operations.Functions.Scalar;

using System.Numerics;
using Domain.Calculator.Values;

public sealed class PowFunction : IFunction
{
    public string Name => "pow";
    public int Arity => 2;
    public FunctionCategory Category => FunctionCategory.Arithmetic;
    public string Signature => "pow(x, y)";

    public string Description =>
        "Exponentiation x^y. Equivalent to the ^ operator but usable as a function, for " +
        "example as an argument to another function, and just as complex-aware: a complex " +
        "x or y (pow(2, _i)) is accepted the same way ^ accepts it. No domain restriction " +
        "on plain reals: negative and fractional exponents are accepted (a negative base " +
        "with a fractional exponent may return NaN, same as Math.Pow).";

    public IReadOnlyList<string> Examples => new[]
    {
        "pow(2, 10) → 1024",
        "pow(9, 0.5) → 3",
        "pow(2, _i) → 0.7692 + 0.6390i",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        if (arguments[0] is NumberValue x && arguments[1] is NumberValue y)
            return new NumberValue(Math.Pow(x.Number, y.Number));

        if (ValueArithmetic.TryToComplex(arguments[0], out var a) &&
            ValueArithmetic.TryToComplex(arguments[1], out var b))
            return ComplexValue.Of(Complex.Pow(a, b));

        throw new InvalidOperationException("pow requires two numbers.");
    }
}
