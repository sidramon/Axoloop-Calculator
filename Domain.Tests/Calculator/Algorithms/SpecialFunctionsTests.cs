namespace Domain.Tests.Calculator.Algorithms;

using Domain.Calculator.Algorithms;
using FluentAssertions;

public class SpecialFunctionsTests
{
    // ---- erf ----

    [Fact]
    public void Erf_Zero_ReturnsZero()
    {
        // The Abramowitz & Stegun 7.1.26 approximation itself is only accurate to ~1.5e-7
        // (its documented max error) -- 1e-7 exercises that bound rather than demanding
        // more precision than the approximation is designed to give.
        SpecialFunctions.Erf(0).Should().BeApproximately(0, 1e-7);
    }

    [Fact]
    public void Erf_PositiveInfinity_ApproachesOne()
    {
        SpecialFunctions.Erf(double.PositiveInfinity).Should().BeApproximately(1, 1e-10);
    }

    [Fact]
    public void Erf_NegativeInfinity_ApproachesNegativeOne()
    {
        SpecialFunctions.Erf(double.NegativeInfinity).Should().BeApproximately(-1, 1e-10);
    }

    [Fact]
    public void Erf_LargeFiniteX_IsWithinToleranceOfOne()
    {
        // erf saturates to 1 well before double overflow -- no need for actual infinity.
        SpecialFunctions.Erf(6).Should().BeApproximately(1, 1e-10);
    }

    [Fact]
    public void Erf_IsOddFunction()
    {
        SpecialFunctions.Erf(-1.5).Should().BeApproximately(-SpecialFunctions.Erf(1.5), 1e-10);
    }

    // Reference values from standard erf tables (e.g. Abramowitz & Stegun table 7.1).
    [Theory]
    [InlineData(0.5, 0.5204998778)]
    [InlineData(1.0, 0.8427007929)]
    [InlineData(1.5, 0.9661051465)]
    [InlineData(2.0, 0.9953222650)]
    public void Erf_TabulatedValues_MatchesReferenceWithinOneEMinusSix(double x, double expected)
    {
        SpecialFunctions.Erf(x).Should().BeApproximately(expected, 1e-6);
    }

    [Fact]
    public void Erfc_IsOneMinusErf()
    {
        SpecialFunctions.Erfc(1.0).Should().BeApproximately(1 - SpecialFunctions.Erf(1.0), 1e-10);
    }

    // ---- gamma ----

    [Theory]
    [InlineData(1, 1)] // gamma(1) = 0!
    [InlineData(2, 1)] // gamma(2) = 1!
    [InlineData(5, 24)] // gamma(5) = 4!
    [InlineData(10, 362880)] // gamma(10) = 9!
    public void Gamma_PositiveInteger_MatchesFactorialOfNMinusOne(int n, double expectedFactorial)
    {
        // Relative, not absolute, tolerance: the Lanczos approximation's ~1e-15 accuracy is
        // a relative bound, so the acceptable absolute error scales with the expected
        // magnitude (e.g. ~4e-10 absolute at 362880, negligible relative to it).
        var relativeError = Math.Abs(SpecialFunctions.Gamma(n) - expectedFactorial) / expectedFactorial;
        relativeError.Should().BeLessThan(1e-9);
    }

    [Fact]
    public void Gamma_OneHalf_ReturnsSqrtPi()
    {
        SpecialFunctions.Gamma(0.5).Should().BeApproximately(Math.Sqrt(Math.PI), 1e-13);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5)]
    public void Gamma_NonPositiveInteger_Throws(double x)
    {
        var act = () => SpecialFunctions.Gamma(x);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Gamma_RecurrenceRelation_HoldsForNonIntegerArgument()
    {
        // gamma(x+1) = x * gamma(x), the defining recurrence -- checked at a non-integer x
        // so it isn't just re-testing the factorial special case above.
        const double x = 2.7;
        SpecialFunctions.Gamma(x + 1).Should().BeApproximately(x * SpecialFunctions.Gamma(x), 1e-9);
    }

    // ---- lgamma ----

    [Fact]
    public void LogGamma_MatchesLogOfGamma_ForModerateArgument()
    {
        SpecialFunctions.LogGamma(5).Should().BeApproximately(Math.Log(SpecialFunctions.Gamma(5)), 1e-9);
    }

    [Fact]
    public void LogGamma_LargeArgument_StaysFiniteWhereGammaOverflows()
    {
        SpecialFunctions.Gamma(200).Should().Be(double.PositiveInfinity);
        double.IsFinite(SpecialFunctions.LogGamma(200)).Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void LogGamma_NonPositiveInteger_Throws(double x)
    {
        var act = () => SpecialFunctions.LogGamma(x);

        act.Should().Throw<InvalidOperationException>();
    }
}
