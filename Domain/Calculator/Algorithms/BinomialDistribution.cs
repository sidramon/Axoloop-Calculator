namespace Domain.Calculator.Algorithms;

/// <summary>Pmf/Cdf of the binomial distribution, built on <see cref="Combinatorics.Choose"/>.</summary>
public static class BinomialDistribution
{
    public static double Pmf(int k, int n, double p)
    {
        Validate(k, n, p);
        return Combinatorics.Choose(n, k) * Math.Pow(p, k) * Math.Pow(1 - p, n - k);
    }

    /// <summary>P(X &lt;= k), by direct summation of <see cref="Pmf"/> from 0 to k.</summary>
    public static double Cdf(int k, int n, double p)
    {
        Validate(k, n, p);

        double sum = 0;
        for (var i = 0; i <= k; i++)
            sum += Pmf(i, n, p);
        return sum;
    }

    private static void Validate(int k, int n, double p)
    {
        if (n < 0)
            throw new InvalidOperationException("n must be non-negative.");
        if (p < 0 || p > 1)
            throw new InvalidOperationException("p must be between 0 and 1.");
        if (k < 0 || k > n)
            throw new InvalidOperationException($"k must be between 0 and n ({n}).");
    }
}
