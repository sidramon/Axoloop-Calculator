namespace Domain.Calculator.Algorithms;

/// <summary>Pdf/Cdf/inverse-Cdf of the normal distribution, built on <see cref="SpecialFunctions.Erf"/>.</summary>
public static class NormalDistribution
{
    public static double Pdf(double x, double mu, double sigma)
    {
        RequirePositiveSigma(sigma);
        var z = (x - mu) / sigma;
        return Math.Exp(-0.5 * z * z) / (sigma * Math.Sqrt(2 * Math.PI));
    }

    public static double Cdf(double x, double mu, double sigma)
    {
        RequirePositiveSigma(sigma);
        return 0.5 * (1 + SpecialFunctions.Erf((x - mu) / (sigma * Math.Sqrt(2))));
    }

    private const int MaxBisectionIterations = 100;

    /// <summary>
    /// The p-quantile, by bisection on <see cref="Cdf"/> — there is no closed form for the
    /// inverse error function in terms of the erf/gamma building blocks this task is
    /// scoped to, so this numerically inverts the (already-approximate) Cdf instead. The
    /// search bracket is ±40 standard deviations, which is astronomically far into either
    /// tail (Cdf is 0 or 1 there to far more than double precision) and safely brackets
    /// any p in (0, 1) representable as a double.
    /// </summary>
    public static double InverseCdf(double p, double mu, double sigma)
    {
        RequirePositiveSigma(sigma);
        if (p <= 0 || p >= 1)
            throw new InvalidOperationException("norminv requires p strictly between 0 and 1.");

        var lower = mu - 40 * sigma;
        var upper = mu + 40 * sigma;

        for (var i = 0; i < MaxBisectionIterations; i++)
        {
            var mid = (lower + upper) / 2;
            if (Cdf(mid, mu, sigma) < p) lower = mid;
            else upper = mid;
        }

        return (lower + upper) / 2;
    }

    private static void RequirePositiveSigma(double sigma)
    {
        if (sigma <= 0)
            throw new InvalidOperationException("sigma must be positive.");
    }
}
