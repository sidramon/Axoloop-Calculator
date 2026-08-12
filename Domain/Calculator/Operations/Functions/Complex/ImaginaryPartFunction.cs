namespace Domain.Calculator.Operations.Functions.Complex;

using Domain.Calculator.Values;

public sealed class ImaginaryPartFunction : IFunction
{
    public string Name => "imag";
    public int Arity => 1;
    public FunctionCategory Category => FunctionCategory.Arithmetic;
    public string Signature => "imag(z)";

    public string Description =>
        "Imaginary part of a number, as a real. Accepts a plain real too — imag(x) is 0 " +
        "— so callers don't need to know in advance whether a value is complex.";

    public IReadOnlyList<string> Examples => new[]
    {
        "imag(3 + 2*_i) → 2",
        "imag(5) → 0",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        if (!ValueArithmetic.TryToComplex(arguments[0], out var c))
            throw new InvalidOperationException("imag requires a number.");
        return new NumberValue(c.Imaginary);
    }
}
