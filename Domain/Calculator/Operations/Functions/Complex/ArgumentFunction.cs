namespace Domain.Calculator.Operations.Functions.Complex;

using Domain.Calculator.Values;

public sealed class ArgumentFunction : IFunction
{
    public string Name => "arg";
    public int Arity => 1;
    public FunctionCategory Category => FunctionCategory.Arithmetic;
    public string Signature => "arg(z)";

    public string Description =>
        "Argument (angle from the positive real axis, in radians, range (-pi, pi]) of a " +
        "number. Accepts a plain real too: arg(x) is 0 for positive x and pi for negative x.";

    public IReadOnlyList<string> Examples => new[]
    {
        "arg(_i) → 1.5708",
        "arg(-1) → 3.1416",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        if (!ValueArithmetic.TryToComplex(arguments[0], out var c))
            throw new InvalidOperationException("arg requires a number.");
        return new NumberValue(c.Phase);
    }
}
