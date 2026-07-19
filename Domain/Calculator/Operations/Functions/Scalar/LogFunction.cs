namespace Domain.Calculator.Operations.Functions.Scalar;

using Domain.Calculator.Values;

public sealed class LogFunction : IFunction
{
    public string Name => "log";
    public int Arity => 2;
    public FunctionCategory Category => FunctionCategory.Arithmetic;
    public string Signature => "log(x, base)";

    public string Description =>
        "Logarithm of x in the given base. Requires x > 0, and base > 0 with base != 1 " +
        "(a base of 1 or a negative base makes the logarithm mathematically undefined).";

    public IReadOnlyList<string> Examples => new[]
    {
        "log(8, 2) → 3",
        "log(8, 1) → Error: log base must be positive and different from 1.",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        if (arguments[0] is not NumberValue x || arguments[1] is not NumberValue b)
            throw new InvalidOperationException("log requires two numbers.");
        if (x.Number <= 0)
            throw new InvalidOperationException("log requires a positive number.");
        if (b.Number <= 0 || b.Number == 1)
            throw new InvalidOperationException("log base must be positive and different from 1.");
        return new NumberValue(Math.Log(x.Number, b.Number));
    }
}
