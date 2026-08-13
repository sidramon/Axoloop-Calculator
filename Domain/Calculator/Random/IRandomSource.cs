namespace Domain.Calculator.Random;

/// <summary>
/// The one door through which every random-number <c>IFunction</c> reaches actual
/// randomness. Injected rather than let each function <c>new</c> up its own
/// <see cref="System.Random"/>: a single shared instance, registered once at the
/// composition root, is what makes <c>seed(k)</c> affect every subsequent draw across
/// every random function, and what makes a scripted implementation usable in tests to
/// verify a function's wiring without depending on real randomness.
///
/// Implementations are NOT required to validate their arguments (e.g. minInclusive
/// &lt;= maxInclusive, stdDev &gt; 0) — that is the calling <c>IFunction</c>'s job, so a
/// minimal scripted test double doesn't need to re-implement every business rule just
/// to satisfy the interface.
/// </summary>
public interface IRandomSource
{
    /// <summary>A uniform random real in [0, 1).</summary>
    double NextDouble();

    /// <summary>A uniform random integer in [minInclusive, maxInclusive] — both ends included.</summary>
    int NextInt(int minInclusive, int maxInclusive);

    /// <summary>A random real drawn from a normal distribution with the given mean and standard deviation.</summary>
    double NextGaussian(double mean, double stdDev);

    /// <summary>Reseeds the source so the sequence of draws from this point on is reproducible.</summary>
    void Seed(int seed);
}
