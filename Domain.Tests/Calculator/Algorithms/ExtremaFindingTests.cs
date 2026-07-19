namespace Domain.Tests.Calculator.Algorithms;

using Domain.Calculator.Algorithms;
using Domain.Calculator.Plotting;
using FluentAssertions;

public class ExtremaFindingTests
{
    private static IReadOnlyList<PlotPoint> Sample(Func<double, double?> f, double xMin, double xMax, int count)
    {
        var points = new List<PlotPoint>(count);
        var step = (xMax - xMin) / (count - 1);
        for (var i = 0; i < count; i++)
        {
            var x = xMin + step * i;
            points.Add(new PlotPoint(x, f(x)));
        }
        return points;
    }

    [Fact]
    public void Find_Sine_FindsMaximaAndMinimaAtTheRightLocationsWithTheRightKind()
    {
        var samples = Sample(x => Math.Sin(x), -10, 10, 2000);

        var extrema = ExtremaFinding.Find(samples, 1e-9);

        var maxima = extrema.Where(e => e.Kind == ExtremumKind.Maximum).OrderBy(e => e.X).ToList();
        var minima = extrema.Where(e => e.Kind == ExtremumKind.Minimum).OrderBy(e => e.X).ToList();

        maxima.Should().HaveCount(3);
        minima.Should().HaveCount(3);

        var expectedMaxima = new[] { -1.5 * Math.PI, 0.5 * Math.PI, 2.5 * Math.PI };
        var expectedMinima = new[] { -2.5 * Math.PI, -0.5 * Math.PI, 1.5 * Math.PI };

        for (var i = 0; i < 3; i++)
        {
            maxima[i].X.Should().BeApproximately(expectedMaxima[i], 1e-3);
            maxima[i].Y.Should().BeApproximately(1, 1e-3);
            minima[i].X.Should().BeApproximately(expectedMinima[i], 1e-3);
            minima[i].Y.Should().BeApproximately(-1, 1e-3);
        }
    }

    [Fact]
    public void Find_Parabola_FindsExactlyOneMinimumAtZero()
    {
        double? f(double x) => x * x;
        var samples = Sample(f, -5, 5, 41);

        var extrema = ExtremaFinding.Find(samples, 1e-9);

        extrema.Should().ContainSingle();
        extrema[0].Kind.Should().Be(ExtremumKind.Minimum);
        extrema[0].X.Should().BeApproximately(0, 1e-9);
        extrema[0].Y.Should().BeApproximately(0, 1e-9);
    }

    [Fact]
    public void Find_AsymptoteGapBetweenBranches_DetectsNoFalseExtremum()
    {
        // Mirrors what the sampling pipeline produces for 1/x once the asymptote-break
        // heuristic has run: monotonically decreasing on each branch, separated by a gap.
        // Naively bridging the gap would look like a slope-sign change, but there is no
        // turning point here.
        var samples = new List<PlotPoint>
        {
            new(-1, -1),
            new(-0.5, -2),
            new(-0.01, -100),
            new(0, null),
            new(0.01, 100),
            new(0.5, 2),
            new(1, 1),
        };

        var extrema = ExtremaFinding.Find(samples, 1e-9);

        extrema.Should().BeEmpty();
    }

    [Fact]
    public void Find_MonotonicFunction_ReturnsEmptyList()
    {
        double? f(double x) => 2 * x + 1;
        var samples = Sample(f, -5, 5, 41);

        var extrema = ExtremaFinding.Find(samples, 1e-9);

        extrema.Should().BeEmpty();
    }

    [Fact]
    public void Find_ParabolicInterpolation_LocatesVertexMuchMorePreciselyThanTheSampleStep()
    {
        double? f(double x) => -(x - 3) * (x - 3);
        const double xMin = 0;
        const double xMax = 6;
        const int count = 13; // coarse: step = 0.5
        var step = (xMax - xMin) / (count - 1);
        var samples = Sample(f, xMin, xMax, count);

        var extrema = ExtremaFinding.Find(samples, 1e-9);

        extrema.Should().ContainSingle();
        extrema[0].Kind.Should().Be(ExtremumKind.Maximum);
        Math.Abs(extrema[0].X - 3).Should().BeLessThan(step / 100);
    }
}
