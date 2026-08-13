namespace Domain.Tests.Calculator.Random;

using Domain.Calculator.Random;
using FluentAssertions;

public class SystemRandomSourceTests
{
    // ---- Reproducibility: the property the whole task hinges on ----

    [Fact]
    public void Seed_TwoSourcesWithSameSeed_ProduceIdenticalDoubleSequences()
    {
        var a = new SystemRandomSource();
        var b = new SystemRandomSource();
        a.Seed(42);
        b.Seed(42);

        var sequenceA = Enumerable.Range(0, 20).Select(_ => a.NextDouble()).ToList();
        var sequenceB = Enumerable.Range(0, 20).Select(_ => b.NextDouble()).ToList();

        sequenceA.Should().Equal(sequenceB);
    }

    [Fact]
    public void Seed_TwoSourcesWithSameSeed_ProduceIdenticalIntSequences()
    {
        var a = new SystemRandomSource();
        var b = new SystemRandomSource();
        a.Seed(7);
        b.Seed(7);

        var sequenceA = Enumerable.Range(0, 20).Select(_ => a.NextInt(1, 100)).ToList();
        var sequenceB = Enumerable.Range(0, 20).Select(_ => b.NextInt(1, 100)).ToList();

        sequenceA.Should().Equal(sequenceB);
    }

    [Fact]
    public void Seed_TwoSourcesWithSameSeed_ProduceIdenticalGaussianSequences()
    {
        var a = new SystemRandomSource();
        var b = new SystemRandomSource();
        a.Seed(123);
        b.Seed(123);

        var sequenceA = Enumerable.Range(0, 20).Select(_ => a.NextGaussian(0, 1)).ToList();
        var sequenceB = Enumerable.Range(0, 20).Select(_ => b.NextGaussian(0, 1)).ToList();

        sequenceA.Should().Equal(sequenceB);
    }

    [Fact]
    public void Seed_DifferentSeeds_ProduceDifferentSequences()
    {
        var a = new SystemRandomSource();
        var b = new SystemRandomSource();
        a.Seed(1);
        b.Seed(2);

        var sequenceA = Enumerable.Range(0, 10).Select(_ => a.NextDouble()).ToList();
        var sequenceB = Enumerable.Range(0, 10).Select(_ => b.NextDouble()).ToList();

        sequenceA.Should().NotEqual(sequenceB);
    }

    [Fact]
    public void Seed_ReseedingTheSameInstance_RestartsTheSequence()
    {
        var source = new SystemRandomSource();
        source.Seed(99);
        var first = Enumerable.Range(0, 10).Select(_ => source.NextDouble()).ToList();

        source.Seed(99);
        var second = Enumerable.Range(0, 10).Select(_ => source.NextDouble()).ToList();

        first.Should().Equal(second);
    }

    // ---- Bounds and distribution shape (statistical, seeded for determinism) ----

    [Fact]
    public void NextDouble_ManyDraws_StaysWithinZeroToOneExclusive()
    {
        var source = new SystemRandomSource();
        source.Seed(1);

        for (var i = 0; i < 100_000; i++)
        {
            var value = source.NextDouble();
            value.Should().BeInRange(0, 1);
            value.Should().BeLessThan(1);
        }
    }

    [Fact]
    public void NextInt_ManyDraws_NeverLeavesRangeAndCoversBothExtremes()
    {
        var source = new SystemRandomSource();
        source.Seed(2);

        var sawMin = false;
        var sawMax = false;
        for (var i = 0; i < 10_000; i++)
        {
            var value = source.NextInt(1, 6);
            value.Should().BeInRange(1, 6);
            if (value == 1) sawMin = true;
            if (value == 6) sawMax = true;
        }

        sawMin.Should().BeTrue();
        sawMax.Should().BeTrue();
    }

    [Fact]
    public void NextInt_DegenerateSingleValueRange_AlwaysReturnsThatValue()
    {
        var source = new SystemRandomSource();
        source.Seed(3);

        for (var i = 0; i < 100; i++)
            source.NextInt(5, 5).Should().Be(5);
    }

    [Fact]
    public void NextGaussian_ManyDraws_MatchesRequestedMeanAndStdDev()
    {
        var source = new SystemRandomSource();
        source.Seed(4);

        var draws = Enumerable.Range(0, 50_000).Select(_ => source.NextGaussian(5, 2)).ToArray();
        var mean = draws.Average();
        var variance = draws.Select(x => (x - mean) * (x - mean)).Average();

        mean.Should().BeApproximately(5, 0.05);
        Math.Sqrt(variance).Should().BeApproximately(2, 0.05);
    }
}
