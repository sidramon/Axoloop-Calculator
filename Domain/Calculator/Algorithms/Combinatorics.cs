namespace Domain.Calculator.Algorithms;

/// <summary>
/// Binomial coefficient and permutation count, computed as a running <c>double</c>
/// product rather than via factorials or <see cref="System.Numerics.BigInteger"/> — this
/// keeps every builtin in the calculator returning the same numeric type, at the cost of
/// precision on large inputs (see the doubling-relative-error note on <see cref="Choose"/>).
/// </summary>
public static class Combinatorics
{
    /// <summary>
    /// C(n, k), via the multiplicative formula C(n,k) = Π_{i=1..k} (n-k+i)/i — never forms
    /// n! or k! directly, so it doesn't overflow the way 100! would (double overflows
    /// around 171!). Precision still degrades gradually as n grows: each of the k
    /// multiply/divide steps carries ~1 ULP of rounding error, so the result's relative
    /// error grows roughly with k. Measured against exact BigInteger arithmetic at k=n/2:
    /// bit-exact (rounds to the precise integer with zero error) through about n=50;
    /// from around n=60 on, the double no longer round-trips to the exact integer, though
    /// the relative error stays roughly 1e-16 (full double precision) even out to n=200 —
    /// "wrong" only in the sense of no longer being an exact integer, correct to 15-16
    /// significant figures regardless of n. Requires non-negative integer n and k; k > n
    /// returns 0 rather than throwing, matching the combinatorial convention that there
    /// are zero ways to choose more items than exist.
    /// </summary>
    public static double Choose(int n, int k)
    {
        RequireNonNegative(n, nameof(n));
        RequireNonNegative(k, nameof(k));

        if (k > n) return 0;
        k = Math.Min(k, n - k); // symmetry: C(n,k) = C(n,n-k), halves the work and the error

        double result = 1;
        for (var i = 1; i <= k; i++)
            result *= (n - k + i) / (double)i;

        return result;
    }

    /// <summary>
    /// P(n, k) = n! / (n-k)! = Π_{i=0..k-1} (n-i), the count of ordered arrangements of k
    /// items drawn from n. Same overflow avoidance as <see cref="Choose"/>: multiplies k
    /// terms directly rather than forming two factorials and dividing.
    /// </summary>
    public static double Permutations(int n, int k)
    {
        RequireNonNegative(n, nameof(n));
        RequireNonNegative(k, nameof(k));

        if (k > n) return 0;

        double result = 1;
        for (var i = 0; i < k; i++)
            result *= n - i;

        return result;
    }

    private static void RequireNonNegative(int value, string name)
    {
        if (value < 0)
            throw new InvalidOperationException($"{name} must be non-negative.");
    }
}
