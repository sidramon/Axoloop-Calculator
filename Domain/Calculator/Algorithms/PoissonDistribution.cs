namespace Domain.Calculator.Algorithms;

/// <summary>
/// Pmf/Cdf of the Poisson distribution. Computed in log space via
/// <see cref="SpecialFunctions.LogGamma"/> (k! = Gamma(k+1)) rather than
/// <c>Math.Exp(-lambda) * Math.Pow(lambda, k) / factorial(k)</c> directly — lambda^k and
/// k! both overflow a double well before their ratio would, for a large enough k.
/// </summary>
public static class PoissonDistribution
{
    public static double Pmf(int k, double lambda)
    {
        Validate(k, lambda);
        var logPmf = k * Math.Log(lambda) - lambda - SpecialFunctions.LogGamma(k + 1);
        return Math.Exp(logPmf);
    }

    /// <summary>P(X &lt;= k), by direct summation of <see cref="Pmf"/> from 0 to k.</summary>
    public static double Cdf(int k, double lambda)
    {
        Validate(k, lambda);

        double sum = 0;
        for (var i = 0; i <= k; i++)
            sum += Pmf(i, lambda);
        return sum;
    }

    private static void Validate(int k, double lambda)
    {
        if (lambda <= 0)
            throw new InvalidOperationException("lambda must be positive.");
        if (k < 0)
            throw new InvalidOperationException("k must be non-negative.");
    }
}
