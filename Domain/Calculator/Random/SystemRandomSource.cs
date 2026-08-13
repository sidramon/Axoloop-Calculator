namespace Domain.Calculator.Random;

/// <summary>
/// Default <see cref="IRandomSource"/>, backed by <see cref="System.Random"/>. One
/// instance is created at the composition root and shared by every random-number
/// <c>IFunction</c> — see <see cref="IRandomSource"/> for why that sharing matters.
/// </summary>
public sealed class SystemRandomSource : IRandomSource
{
    private System.Random _random = new();

    public double NextDouble() => _random.NextDouble();

    // System.Random.Next(min, max)'s upper bound is EXCLUSIVE; this interface's is
    // inclusive, hence the + 1. (Not safe if maxInclusive is int.MaxValue — not a
    // practical concern for a calculator's randint bounds.)
    public int NextInt(int minInclusive, int maxInclusive) => _random.Next(minInclusive, maxInclusive + 1);

    // Box-Muller transform. u1 is drawn from (0, 1] rather than [0, 1) specifically so
    // Math.Log never sees exactly 0.
    public double NextGaussian(double mean, double stdDev)
    {
        var u1 = 1.0 - _random.NextDouble();
        var u2 = _random.NextDouble();
        var standardNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
        return mean + stdDev * standardNormal;
    }

    public void Seed(int seed) => _random = new System.Random(seed);
}
