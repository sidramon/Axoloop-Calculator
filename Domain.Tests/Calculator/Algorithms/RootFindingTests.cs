namespace Domain.Tests.Calculator.Algorithms;

using Domain.Calculator.Algorithms;
using Domain.Calculator.Plotting;
using FluentAssertions;

public class RootFindingTests
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
    public void FindSignChanges_QuadraticWithTwoRoots_FindsBothRoots()
    {
        double? f(double x) => x * x - 4;
        var samples = Sample(f, -5, 5, 41);

        var roots = RootFinding.FindSignChanges(samples, f, 1e-9);

        roots.Should().HaveCount(2);
        roots.Should().Contain(r => Math.Abs(r - -2) < 1e-6);
        roots.Should().Contain(r => Math.Abs(r - 2) < 1e-6);
    }

    [Fact]
    public void FindSignChanges_NoRealRoots_ReturnsEmpty()
    {
        double? f(double x) => x * x + 1;
        var samples = Sample(f, -5, 5, 41);

        var roots = RootFinding.FindSignChanges(samples, f, 1e-9);

        roots.Should().BeEmpty();
    }

    [Fact]
    public void FindSignChanges_AsymptoteBreakBetweenBranches_DoesNotReportFalseRoot()
    {
        // Mirrors what the sampling pipeline produces for 1/x once the asymptote-break
        // heuristic has run: the point nearest the pole is nulled, separating the negative
        // and positive branches instead of connecting them across zero.
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

        var roots = RootFinding.FindSignChanges(samples, x => x == 0 ? null : 1 / x, 1e-9);

        roots.Should().BeEmpty();
    }
}
