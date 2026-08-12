namespace Domain.Calculator.Operations.Functions.Complex;

using Domain.Calculator.Values;

public sealed class ConjugateFunction : IFunction
{
    public string Name => "conj";
    public int Arity => 1;
    public FunctionCategory Category => FunctionCategory.Arithmetic;
    public string Signature => "conj(z)";

    public string Description =>
        "Complex conjugate: negates the imaginary part. Accepts a plain real too — " +
        "conj(x) is just x — so callers don't need to know in advance whether a value " +
        "is complex.";

    public IReadOnlyList<string> Examples => new[]
    {
        "conj(3 + 2*_i) → 3 - 2i",
        "conj(5) → 5",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        if (!ValueArithmetic.TryToComplex(arguments[0], out var c))
            throw new InvalidOperationException("conj requires a number.");
        return ComplexValue.Of(c.Real, -c.Imaginary);
    }
}
