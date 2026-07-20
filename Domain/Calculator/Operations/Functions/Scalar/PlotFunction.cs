namespace Domain.Calculator.Operations.Functions.Scalar;

using Domain.Calculator.Values;

public sealed class PlotFunction : IFunction
{
    private const int SampleCount = 100;

    public string Name => "plot";
    public int Arity => 3;
    public FunctionCategory Category => FunctionCategory.Arithmetic;
    public string Signature => "plot(f, xMin, xMax)";

    public string Description =>
        "Samples f at 100 evenly spaced points across [xMin, xMax] and returns them as an " +
        "N x 2 matrix: column 0 is x, column 1 is f(x). A point where f throws or returns a " +
        "non-finite value is recorded as NaN rather than stopping the sampling. f must be a " +
        "function value of arity 1. This returns raw sample points as a value, distinct from " +
        "the /plot and /plotweb commands, which render a visual plot with gap/asymptote " +
        "detection, zero-finding, and extrema.";

    public IReadOnlyList<string> Examples => new[]
    {
        "f(x) := x^2 :: plot(f, -2, 2) → 100x2 matrix of [x, x^2] sample pairs",
        "plot(sin, 0, 3.14159) → 100x2 matrix of [x, sin(x)] sample pairs",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        if (arguments[0] is not FunctionValue function)
            throw new InvalidOperationException("plot requires a function as its first argument.");
        if (function.Arity != 1)
            throw new InvalidOperationException(
                $"plot requires a single-parameter function, but '{function.Name}' takes {function.Arity}.");

        var xMin = FunctionArguments.RequireNumber(arguments[1], "plot");
        var xMax = FunctionArguments.RequireNumber(arguments[2], "plot");

        if (!(xMax > xMin))
            throw new InvalidOperationException($"Invalid domain: xMin ({xMin}) must be less than xMax ({xMax}).");

        var data = new double[SampleCount, 2];
        var step = (xMax - xMin) / (SampleCount - 1);

        for (var i = 0; i < SampleCount; i++)
        {
            var x = xMin + step * i;
            data[i, 0] = x;
            data[i, 1] = Evaluate(function, x);
        }

        return new MatrixValue(data);
    }

    private static double Evaluate(FunctionValue function, double x)
    {
        Value result;
        try
        {
            result = function.Invoke(new Value[] { new NumberValue(x) });
        }
        catch
        {
            return double.NaN;
        }

        return result is NumberValue number ? number.Number : double.NaN;
    }
}
