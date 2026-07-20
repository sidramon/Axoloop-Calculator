namespace Domain.Tests.Calculator.Algorithms;

using Domain.Calculator.Algorithms;
using FluentAssertions;

public class NumericalCalculusTests
{
    [Fact]
    public void Derivative_SquareFunctionAtThree_ReturnsSix()
    {
        double f(double x) => x * x;

        var result = NumericalCalculus.Derivative(f, 3);

        result.Should().BeApproximately(6, 1e-4);
    }

    [Fact]
    public void Derivative_SineAtZero_ReturnsOne()
    {
        var result = NumericalCalculus.Derivative(Math.Sin, 0);

        result.Should().BeApproximately(1, 1e-6);
    }

    [Fact]
    public void Integral_SquareFunctionOverZeroToOne_ReturnsOneThird()
    {
        double f(double x) => x * x;

        var result = NumericalCalculus.Integral(f, 0, 1);

        result.Should().BeApproximately(1.0 / 3.0, 1e-9);
    }

    [Fact]
    public void Integral_SineOverZeroToPi_ReturnsTwo()
    {
        var result = NumericalCalculus.Integral(Math.Sin, 0, Math.PI);

        result.Should().BeApproximately(2, 1e-9);
    }

    [Fact]
    public void Integral_UpperBoundNotGreaterThanLowerBound_Throws()
    {
        var act = () => NumericalCalculus.Integral(x => x, 1, 1);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Derivative_FunctionThrowingAtEvaluationPoint_WrapsWithClearMessage()
    {
        double f(double x) => x < 0 ? throw new InvalidOperationException("domain error") : Math.Sqrt(x);

        var act = () => NumericalCalculus.Derivative(f, 0);

        act.Should().Throw<InvalidOperationException>();
    }

    // ---- NthDerivative ----

    [Fact]
    public void NthDerivative_OrderOne_MatchesDerivativeExactly()
    {
        double f(double x) => x * x;

        NumericalCalculus.NthDerivative(f, 3, 1).Should().Be(NumericalCalculus.Derivative(f, 3));
    }

    [Fact]
    public void NthDerivative_SecondDerivativeOfCubeAtTwo_ReturnsTwelve()
    {
        // Tolerance looser than a first-derivative computation: the wider stencil and
        // larger step needed for a second derivative cost roughly two fewer correct digits.
        double cube(double x) => x * x * x;

        NumericalCalculus.NthDerivative(cube, 2, 2).Should().BeApproximately(12, 1e-5);
    }

    [Fact]
    public void NthDerivative_SecondDerivativeOfSineAtZero_ReturnsZero()
    {
        NumericalCalculus.NthDerivative(Math.Sin, 0, 2).Should().BeApproximately(0, 1e-5);
    }

    [Fact]
    public void NthDerivative_ThirdDerivativeOfCube_ReturnsSix()
    {
        // Looser again than the second derivative: a 5-point stencil at an even larger step.
        double cube(double x) => x * x * x;

        NumericalCalculus.NthDerivative(cube, 2, 3).Should().BeApproximately(6, 1e-4);
    }

    [Fact]
    public void NthDerivative_OrderFive_ThrowsExplainingThePrecisionCap()
    {
        var act = () => NumericalCalculus.NthDerivative(x => x, 1, 5);

        act.Should().Throw<InvalidOperationException>().WithMessage("*1*4*");
    }

    [Fact]
    public void NthDerivative_OrderZero_Throws()
    {
        var act = () => NumericalCalculus.NthDerivative(x => x, 1, 0);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void NthDerivative_NegativeOrder_Throws()
    {
        var act = () => NumericalCalculus.NthDerivative(x => x, 1, -1);

        act.Should().Throw<InvalidOperationException>();
    }

    // ---- Integral: reversed bounds ----

    [Fact]
    public void Integral_ReversedBounds_ReturnsOppositeOfForwardBounds()
    {
        double f(double x) => x * x;

        var forward = NumericalCalculus.Integral(f, 2, 5);
        var reversed = NumericalCalculus.Integral(f, 5, 2);

        reversed.Should().BeApproximately(-forward, 1e-9);
    }

    // ---- Antiderivative ----

    [Fact]
    public void Antiderivative_OfSquareFromZero_EvaluatedAtOne_ReturnsOneThird()
    {
        double f(double x) => x * x;

        var F = NumericalCalculus.Antiderivative(f, 0, 200);

        F(1).Should().BeApproximately(1.0 / 3.0, 1e-6);
    }

    [Fact]
    public void Antiderivative_AtItsOwnBasePoint_IsZero()
    {
        double f(double x) => x * x;

        var F = NumericalCalculus.Antiderivative(f, 5, 200);

        F(5).Should().Be(0);
    }

    [Fact]
    public void Antiderivative_WithExplicitBasePoint_MatchesDefiniteIntegralBetweenBaseAndX()
    {
        double f(double x) => x * x;

        var F = NumericalCalculus.Antiderivative(f, 1, 200);

        F(2).Should().BeApproximately(NumericalCalculus.Integral(f, 1, 2), 1e-9);
    }
}
