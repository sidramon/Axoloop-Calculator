namespace Domain.Calculator.Algorithms;

/// <summary>
/// Descriptive statistics over a plain sample array — the numeric core behind the
/// mean/median/mode/variance/etc. builtins. Takes <c>double[]</c> rather than
/// <see cref="Values.MatrixValue"/> so it stays free of the <c>Value</c> layer, matching
/// every other algorithm in this namespace; the <c>IFunction</c> adapters do the
/// <c>MatrixValue.AsVector</c> conversion before calling in.
/// </summary>
public static class DescriptiveStatistics
{
    public static double Mean(double[] v)
    {
        RequireNonEmpty(v);
        return Sum(v) / v.Length;
    }

    /// <summary>Average of the two middle elements when <paramref name="v"/> has even length.</summary>
    public static double Median(double[] v)
    {
        RequireNonEmpty(v);

        var sorted = (double[])v.Clone();
        Array.Sort(sorted);

        var mid = sorted.Length / 2;
        return sorted.Length % 2 == 1
            ? sorted[mid]
            : (sorted[mid - 1] + sorted[mid]) / 2.0;
    }

    /// <summary>
    /// The most frequent value. Ties are broken by returning the smallest value among
    /// those tied for the highest count — arbitrary but deterministic, since "the" mode of
    /// a multimodal sample has no single correct answer.
    /// </summary>
    public static double Mode(double[] v)
    {
        RequireNonEmpty(v);

        var counts = new Dictionary<double, int>();
        foreach (var x in v)
            counts[x] = counts.GetValueOrDefault(x) + 1;

        var bestCount = counts.Values.Max();
        return counts.Where(kv => kv.Value == bestCount).Min(kv => kv.Key);
    }

    /// <summary>
    /// Sample variance (Bessel's correction, divides by n-1) by default — the standard
    /// choice when <paramref name="v"/> is a sample used to estimate a population's
    /// variance, and what Excel's VAR.S, R's var(), and most calculators default to.
    /// Pass <paramref name="population"/>: true for the population variance (divides by
    /// n instead), the right choice only when <paramref name="v"/> IS the entire
    /// population rather than a sample of it. The two differ by a factor of n/(n-1) and
    /// are a common source of "why doesn't this match Excel" confusion if the distinction
    /// goes unstated. Sample variance requires at least 2 points (n-1 = 0 otherwise);
    /// population variance accepts a single point (variance 0).
    /// </summary>
    public static double Variance(double[] v, bool population = false)
    {
        RequireNonEmpty(v);
        if (!population && v.Length < 2)
            throw new InvalidOperationException(
                "Sample variance requires at least 2 values (n-1 would be 0). " +
                "Pass population: true for a single-point population variance.");

        var mean = Mean(v);
        var sumSquaredDeviations = v.Sum(x => (x - mean) * (x - mean));
        var denominator = population ? v.Length : v.Length - 1;
        return sumSquaredDeviations / denominator;
    }

    /// <summary>Square root of <see cref="Variance"/> — same sample-vs-population convention.</summary>
    public static double StandardDeviation(double[] v, bool population = false) =>
        Math.Sqrt(Variance(v, population));

    public static double Range(double[] v)
    {
        RequireNonEmpty(v);
        return v.Max() - v.Min();
    }

    public static double Sum(double[] v)
    {
        RequireNonEmpty(v);
        return v.Sum();
    }

    public static double Product(double[] v)
    {
        RequireNonEmpty(v);
        var product = 1.0;
        foreach (var x in v) product *= x;
        return product;
    }

    public static double Min(double[] v)
    {
        RequireNonEmpty(v);
        return v.Min();
    }

    public static double Max(double[] v)
    {
        RequireNonEmpty(v);
        return v.Max();
    }

    private static void RequireNonEmpty(double[] v)
    {
        if (v.Length == 0)
            throw new InvalidOperationException("Requires a non-empty vector.");
    }
}
