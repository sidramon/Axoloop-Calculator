namespace Domain.Tests.Calculator.Algorithms;

using Domain.Calculator.Algorithms;
using FluentAssertions;

public class LimitEvaluationTests
{
    private const double Tolerance = 1e-6;

    [Fact]
    public void SinXOverX_AtZero_ConvergesToOne()
    {
        double? g(double x) => x == 0 ? null : Math.Sin(x) / x;

        var result = LimitEvaluation.Evaluate(g, 0, Tolerance);

        result.Kind.Should().Be(LimitKind.Converges);
        result.Value.Should().BeApproximately(1, 1e-6);
    }

    [Fact]
    public void SinXOverX_AtZero_NeverSamplesBelowTheCatastrophicCancellationFloor()
    {
        // The classic trap: descending toward h ~ 1e-16 makes sin(x) and x both pure
        // rounding noise, and their ratio becomes arbitrary. Instrument g to record
        // every offset actually sampled and assert none of them went anywhere near
        // machine epsilon.
        var sampledOffsets = new List<double>();

        double? g(double x)
        {
            if (x != 0) sampledOffsets.Add(Math.Abs(x));
            return x == 0 ? null : Math.Sin(x) / x;
        }

        var result = LimitEvaluation.Evaluate(g, 0, Tolerance);

        result.Kind.Should().Be(LimitKind.Converges);
        sampledOffsets.Should().NotBeEmpty();
        sampledOffsets.Min().Should().BeGreaterThanOrEqualTo(1e-6,
            "the descent must stop at the catastrophic-cancellation floor, not push toward 1e-16 where sin(x)/x becomes noise");
    }

    [Fact]
    public void RemovableSingularity_AtTheHole_ConvergesToTheFilledInValue()
    {
        // (x^2 - 4) / (x - 2), undefined exactly at x = 2, but the limit there is 4.
        double? g(double x) => x == 2 ? null : (x * x - 4) / (x - 2);

        var result = LimitEvaluation.Evaluate(g, 2, Tolerance);

        result.Kind.Should().Be(LimitKind.Converges);
        result.Value.Should().BeApproximately(4, 1e-6);
    }

    [Fact]
    public void OneOverX_AtZero_DivergesDifferentlyOnEachSide()
    {
        double? g(double x) => x == 0 ? null : 1.0 / x;

        var result = LimitEvaluation.Evaluate(g, 0, Tolerance);

        result.Kind.Should().Be(LimitKind.OneSidedDiffer);
        result.Value.Should().BeNull();
        result.LeftValue.Should().NotBeNull();
        result.RightValue.Should().NotBeNull();
        double.IsNegativeInfinity(result.LeftValue!.Value).Should().BeTrue();
        double.IsPositiveInfinity(result.RightValue!.Value).Should().BeTrue();
    }

    [Fact]
    public void OneOverXSquared_AtZero_DivergesToPositiveInfinityOnBothSides()
    {
        double? g(double x) => x == 0 ? null : 1.0 / (x * x);

        var result = LimitEvaluation.Evaluate(g, 0, Tolerance);

        result.Kind.Should().Be(LimitKind.DivergesToPositiveInfinity);
        double.IsPositiveInfinity(result.Value!.Value).Should().BeTrue();
        double.IsPositiveInfinity(result.LeftValue!.Value).Should().BeTrue();
        double.IsPositiveInfinity(result.RightValue!.Value).Should().BeTrue();
    }

    [Fact]
    public void SinOfOneOverX_AtZero_HasNoLimit()
    {
        double? g(double x) => x == 0 ? null : Math.Sin(1.0 / x);

        var result = LimitEvaluation.Evaluate(g, 0, Tolerance);

        result.Kind.Should().Be(LimitKind.NoLimit);
        result.Value.Should().BeNull();
    }

    [Fact]
    public void OneOverX_AtPositiveInfinity_ConvergesToZero()
    {
        double? g(double x) => x == 0 ? null : 1.0 / x;

        var result = LimitEvaluation.Evaluate(g, double.PositiveInfinity, Tolerance);

        result.Kind.Should().Be(LimitKind.Converges);
        result.Value.Should().BeApproximately(0, 1e-6);
    }

    [Fact]
    public void OneOverX_AtNegativeInfinity_ConvergesToZero()
    {
        double? g(double x) => x == 0 ? null : 1.0 / x;

        var result = LimitEvaluation.Evaluate(g, double.NegativeInfinity, Tolerance);

        result.Kind.Should().Be(LimitKind.Converges);
        result.Value.Should().BeApproximately(0, 1e-6);
    }

    [Fact]
    public void OnePlusOneOverXToTheX_AtPositiveInfinity_ConvergesToE()
    {
        // Classic slow convergence: relative error is only O(1/x), so this needs a much
        // looser tolerance than sin(x)/x's near-exact Richardson extrapolation.
        double? g(double x) => Math.Pow(1 + 1 / x, x);

        var result = LimitEvaluation.Evaluate(g, double.PositiveInfinity, Tolerance);

        result.Kind.Should().Be(LimitKind.Converges);
        result.Value.Should().BeApproximately(Math.E, 1e-2);
    }

    [Fact]
    public void ContinuousPolynomial_LimitEqualsDirectEvaluation()
    {
        double? g(double x) => x * x;

        var result = LimitEvaluation.Evaluate(g, 3, Tolerance);

        result.Kind.Should().Be(LimitKind.Converges);
        result.Value.Should().BeApproximately(9, 1e-6);
    }

    [Fact]
    public void StepFunction_LeftAndRightLimitsDifferAtTheJump()
    {
        double? g(double x) => x < 0 ? -1.0 : 1.0;

        var result = LimitEvaluation.Evaluate(g, 0, Tolerance);

        result.Kind.Should().Be(LimitKind.OneSidedDiffer);
        result.LeftValue.Should().BeApproximately(-1, 1e-6);
        result.RightValue.Should().BeApproximately(1, 1e-6);
    }

    [Fact]
    public void StepFunction_AwayFromTheJump_ConvergesNormally()
    {
        double? g(double x) => x < 0 ? -1.0 : 1.0;

        var result = LimitEvaluation.Evaluate(g, 5, Tolerance);

        result.Kind.Should().Be(LimitKind.Converges);
        result.Value.Should().BeApproximately(1, 1e-6);
    }

    [Fact]
    public void AllPointsOnOneSideUndefined_ThatSideHasNoValue()
    {
        // g is only defined for x >= 0 (e.g. sqrt-like domain restriction); approaching
        // 0 from the left never gets a single defined sample.
        double? g(double x) => x < 0 ? null : x + 1;

        var result = LimitEvaluation.Evaluate(g, 0, Tolerance);

        result.LeftValue.Should().BeNull();
        result.RightValue.Should().NotBeNull();
        result.RightValue!.Value.Should().BeApproximately(1, 1e-6);
        // No coherent BILATERAL conclusion is possible without left-side data, even
        // though the right side alone is well-behaved.
        result.Kind.Should().Be(LimitKind.NoLimit);
    }

    [Fact]
    public void EveryPointUndefined_ProducesNoLimitWithNoValues()
    {
        double? g(double x) => null;

        var result = LimitEvaluation.Evaluate(g, 0, Tolerance);

        result.Kind.Should().Be(LimitKind.NoLimit);
        result.Value.Should().BeNull();
        result.LeftValue.Should().BeNull();
        result.RightValue.Should().BeNull();
    }

    [Fact]
    public void NaNSamples_AreTreatedAsUndefinedNotAsAValue()
    {
        double? g(double x) => double.NaN;

        var result = LimitEvaluation.Evaluate(g, 0, Tolerance);

        result.Kind.Should().Be(LimitKind.NoLimit);
    }
}
