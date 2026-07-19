namespace Domain.Calculator.Operations.Functions.Scalar;

using Domain.Calculator.Values;

public sealed class PowFunction : IFunction
{
    public string Name => "pow";
    public int Arity => 2;
    public FunctionCategory Category => FunctionCategory.Arithmetic;
    public string Signature => "pow(x, y)";

    public string Description =>
        "Exponentiation x^y. Equivalent to the ^ operator but usable as a function, for " +
        "example as an argument to another function. No domain restriction: negative and " +
        "fractional exponents are accepted (a negative base with a fractional exponent " +
        "may return NaN, same as Math.Pow).";

    public IReadOnlyList<string> Examples => new[]
    {
        "pow(2, 10) → 1024",
        "pow(9, 0.5) → 3",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        if (arguments[0] is not NumberValue x || arguments[1] is not NumberValue y)
            throw new InvalidOperationException("pow requires two numbers.");
        return new NumberValue(Math.Pow(x.Number, y.Number));
    }
}
