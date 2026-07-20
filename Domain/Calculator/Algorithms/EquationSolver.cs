namespace Domain.Calculator.Algorithms;

public static class EquationSolver
{
    private const int MaxNewtonIterations = 100;
    private const int MaxBisectionIterations = 200;
    private const double BracketExpansionFactor = 1.6;
    private const int MaxBracketExpansions = 60;
    private const double DivergenceThreshold = 1e12;

    public static double Solve(Func<double, double> f, double target, double initialGuess, double tolerance)
    {
        double Shifted(double x) => NumericFunctionGuard.Evaluate(f, x) - target;

        if (TryNewtonRaphson(Shifted, initialGuess, tolerance, out var newtonRoot))
            return newtonRoot;

        if (TryBisectionFallback(Shifted, initialGuess, tolerance, out var bisectionRoot))
            return bisectionRoot;

        throw new InvalidOperationException(
            $"No root found near x = {initialGuess} for f(x) = {target}.");
    }

    private static bool TryNewtonRaphson(Func<double, double> g, double initialGuess, double tolerance, out double root)
    {
        var x = initialGuess;

        for (var i = 0; i < MaxNewtonIterations; i++)
        {
            var y = g(x);
            if (Math.Abs(y) <= tolerance)
            {
                root = x;
                return true;
            }

            var derivative = CentralDifference(g, x);
            if (Math.Abs(derivative) < MatrixArrays.Tolerance)
                break;

            var next = x - y / derivative;

            if (double.IsNaN(next) || double.IsInfinity(next) || Math.Abs(next - x) > DivergenceThreshold)
                break;

            x = next;
        }

        root = default;
        return false;
    }

    private static double CentralDifference(Func<double, double> g, double x)
    {
        var h = Math.Max(Math.Abs(x), 1) * 1e-6;
        return (g(x + h) - g(x - h)) / (2 * h);
    }

    private static bool TryBisectionFallback(Func<double, double> g, double initialGuess, double tolerance, out double root)
    {
        if (!TryFindBracket(g, initialGuess, out var lower, out var upper, out var yLower))
        {
            root = default;
            return false;
        }

        for (var i = 0; i < MaxBisectionIterations && upper - lower > tolerance; i++)
        {
            var mid = (lower + upper) / 2;
            var yMid = g(mid);

            if (yMid == 0)
            {
                root = mid;
                return true;
            }

            if (Math.Sign(yMid) == Math.Sign(yLower))
            {
                lower = mid;
                yLower = yMid;
            }
            else
            {
                upper = mid;
            }
        }

        root = (lower + upper) / 2;
        return true;
    }

    private static bool TryFindBracket(
        Func<double, double> g, double initialGuess, out double lower, out double upper, out double yLower)
    {
        var step = Math.Max(Math.Abs(initialGuess), 1) * 0.1;
        lower = initialGuess - step;
        upper = initialGuess + step;
        yLower = g(lower);
        var yUpper = g(upper);

        for (var i = 0; i < MaxBracketExpansions; i++)
        {
            if (Math.Sign(yLower) != Math.Sign(yUpper))
                return true;

            step *= BracketExpansionFactor;
            lower = initialGuess - step;
            upper = initialGuess + step;
            yLower = g(lower);
            yUpper = g(upper);
        }

        return false;
    }
}
