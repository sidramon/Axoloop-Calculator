namespace Domain.Calculator.Algorithms;

public static class NumericalCalculus
{
    private const int DefaultIntegrationSubintervals = 1000;
    private const int MinDerivativeOrder = 1;
    private const int MaxDerivativeOrder = 4;

    public static double Derivative(Func<double, double> f, double x)
    {
        var h = Math.Max(Math.Abs(x), 1) * 1e-6;
        var yPlus = NumericFunctionGuard.Evaluate(f, x + h);
        var yMinus = NumericFunctionGuard.Evaluate(f, x - h);
        return (yPlus - yMinus) / (2 * h);
    }

    /// <summary>
    /// The nth derivative of f at x, computed directly from a central-difference stencil
    /// sized to the order (3 points for orders 1-2, 5 points for orders 3-4) — never by
    /// nesting first-derivative computations, which amplifies rounding error by roughly
    /// four orders of magnitude at every level of nesting. The step size grows with the
    /// order to keep truncation and rounding error balanced; precision still degrades as n
    /// grows, which is why orders above 4 are rejected rather than returning noise.
    /// </summary>
    public static double NthDerivative(Func<double, double> f, double x, int n)
    {
        if (n < MinDerivativeOrder || n > MaxDerivativeOrder)
            throw new InvalidOperationException(
                $"Only derivative orders {MinDerivativeOrder} through {MaxDerivativeOrder} are supported " +
                $"(numerical precision does not allow more); got n = {n}.");

        if (n == 1)
            return Derivative(f, x);

        var h = Math.Max(Math.Abs(x), 1) * StepScale(n);
        double At(double offset) => NumericFunctionGuard.Evaluate(f, x + offset * h);

        return n switch
        {
            2 => (At(1) - 2 * At(0) + At(-1)) / (h * h),
            3 => (-At(-2) + 2 * At(-1) - 2 * At(1) + At(2)) / (2 * h * h * h),
            4 => (At(-2) - 4 * At(-1) + 6 * At(0) - 4 * At(1) + At(2)) / (h * h * h * h),
            _ => throw new InvalidOperationException("Unreachable: order already validated."),
        };
    }

    private static double StepScale(int n) => n switch
    {
        2 => 1e-4,
        3 => 1e-3,
        4 => 3e-3,
        _ => throw new InvalidOperationException("Unreachable: order already validated."),
    };

    public static double Integral(Func<double, double> f, double a, double b, int subintervals = DefaultIntegrationSubintervals)
    {
        if (a == b)
            throw new InvalidOperationException($"Invalid integration bounds: a ({a}) must not equal b ({b}).");

        // Reversed bounds are the standard convention: integral(f, b, a) = -integral(f, a, b).
        if (a > b)
            return -Integral(f, b, a, subintervals);

        if (subintervals < 2)
            subintervals = 2;
        if (subintervals % 2 != 0)
            subintervals++;

        var h = (b - a) / subintervals;
        var sum = NumericFunctionGuard.Evaluate(f, a) + NumericFunctionGuard.Evaluate(f, b);

        for (var i = 1; i < subintervals; i++)
        {
            var x = a + i * h;
            var coefficient = i % 2 == 0 ? 2 : 4;
            sum += coefficient * NumericFunctionGuard.Evaluate(f, x);
        }

        return sum * h / 3;
    }

    /// <summary>
    /// Builds an antiderivative F(x) = integral of f from basePoint to x — one antiderivative
    /// among infinitely many differing by a constant, namely the one that vanishes at
    /// basePoint. Each call to the returned function reruns a full quadrature from
    /// basePoint, so subintervals is typically kept lower here than for a single one-shot
    /// definite integral.
    /// </summary>
    public static Func<double, double> Antiderivative(Func<double, double> f, double basePoint, int subintervals) =>
        x => x == basePoint ? 0 : Integral(f, basePoint, x, subintervals);
}
