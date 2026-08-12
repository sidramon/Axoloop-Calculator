namespace Domain.Calculator.Operations.Functions.Complex;

using Domain.Calculator.Values;

public sealed class RealPartFunction : IFunction
{
    public string Name => "real";
    public int Arity => 1;
    public FunctionCategory Category => FunctionCategory.Arithmetic;
    public string Signature => "real(z)";

    public string Description =>
        "Real part of a number. Accepts a plain real too — real(x) is just x — so callers " +
        "don't need to know in advance whether a value is complex.";

    public IReadOnlyList<string> Examples => new[]
    {
        "real(3 + 2*_i) → 3",
        "real(5) → 5",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        if (!ValueArithmetic.TryToComplex(arguments[0], out var c))
            throw new InvalidOperationException("real requires a number.");
        return new NumberValue(c.Real);
    }
}
