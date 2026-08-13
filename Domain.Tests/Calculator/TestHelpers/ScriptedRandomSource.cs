namespace Domain.Tests.Calculator.TestHelpers;

using Domain.Calculator.Random;

/// <summary>
/// A fully deterministic <see cref="IRandomSource"/> for tests: each method draws from
/// its own pre-loaded queue rather than generating anything, so a test can assert
/// exactly what a random-number <c>IFunction</c> produces for a known "roll", and that
/// it forwards its arguments (bounds, mean/stdDev) to the source correctly, without
/// depending on real randomness. Throws if a queue runs out — a test should always
/// script exactly as many values as it expects to be consumed, so an unexpectedly
/// extra draw fails loudly instead of silently returning a stale or default value.
/// </summary>
public sealed class ScriptedRandomSource : IRandomSource
{
    private readonly Queue<double> _doubles;
    private readonly Queue<int> _ints;
    private readonly Queue<double> _gaussians;

    public int? LastSeed { get; private set; }
    public int SeedCallCount { get; private set; }

    public ScriptedRandomSource(
        IEnumerable<double>? doubles = null,
        IEnumerable<int>? ints = null,
        IEnumerable<double>? gaussians = null)
    {
        _doubles = new Queue<double>(doubles ?? Array.Empty<double>());
        _ints = new Queue<int>(ints ?? Array.Empty<int>());
        _gaussians = new Queue<double>(gaussians ?? Array.Empty<double>());
    }

    public double NextDouble() => _doubles.Count > 0
        ? _doubles.Dequeue()
        : throw new InvalidOperationException("ScriptedRandomSource: NextDouble queue exhausted.");

    public int NextInt(int minInclusive, int maxInclusive) => _ints.Count > 0
        ? _ints.Dequeue()
        : throw new InvalidOperationException("ScriptedRandomSource: NextInt queue exhausted.");

    public double NextGaussian(double mean, double stdDev) => _gaussians.Count > 0
        ? _gaussians.Dequeue()
        : throw new InvalidOperationException("ScriptedRandomSource: NextGaussian queue exhausted.");

    public void Seed(int seed)
    {
        LastSeed = seed;
        SeedCallCount++;
    }
}
