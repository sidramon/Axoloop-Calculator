namespace Domain.Calculator.Algorithms;

internal static class NumericFunctionGuard
{
    public static double Evaluate(Func<double, double> f, double x)
    {
        double y;
        try
        {
            y = f(x);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"The function threw while evaluating at x = {x}: {ex.Message}", ex);
        }

        if (double.IsNaN(y) || double.IsInfinity(y))
            throw new InvalidOperationException($"The function returned a non-finite value at x = {x}.");

        return y;
    }
}
