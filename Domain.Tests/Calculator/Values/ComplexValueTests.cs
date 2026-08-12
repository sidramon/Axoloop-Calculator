namespace Domain.Tests.Calculator.Values;

using System.Numerics;
using Domain.Calculator.Values;
using FluentAssertions;

public class ComplexValueTests
{
    [Fact]
    public void Of_NonNegligibleImaginaryPart_ReturnsComplexValue()
    {
        var result = ComplexValue.Of(2, 3);

        result.Should().BeOfType<ComplexValue>();
        var c = (ComplexValue)result;
        c.Real.Should().BeApproximately(2, 1e-10);
        c.Imaginary.Should().BeApproximately(3, 1e-10);
    }

    [Fact]
    public void Of_ImaginaryPartExactlyZero_ReducesToNumberValue()
    {
        var result = ComplexValue.Of(2, 0);

        result.Should().BeOfType<NumberValue>();
        ((NumberValue)result).Number.Should().BeApproximately(2, 1e-10);
    }

    [Fact]
    public void Of_ImaginaryPartWithinTolerance_ReducesToNumberValue()
    {
        // Just under ComplexValue.ReductionTolerance (1e-12): floating-point noise from a
        // real computation, not a genuinely complex result.
        var result = ComplexValue.Of(2, 1e-13);

        result.Should().BeOfType<NumberValue>();
    }

    [Fact]
    public void Of_ImaginaryPartAboveTolerance_StaysComplex()
    {
        var result = ComplexValue.Of(2, 1e-6);

        result.Should().BeOfType<ComplexValue>();
    }

    [Fact]
    public void Of_Complex_RoundTripsThroughSystemNumericsComplex()
    {
        var result = ComplexValue.Of(new Complex(4, 5));

        result.Should().BeOfType<ComplexValue>();
        var c = (ComplexValue)result;
        c.Real.Should().BeApproximately(4, 1e-10);
        c.Imaginary.Should().BeApproximately(5, 1e-10);
    }

    [Fact]
    public void ToComplex_RoundTripsRealAndImaginary()
    {
        var c = new ComplexValue(3, 4).ToComplex();

        c.Real.Should().BeApproximately(3, 1e-10);
        c.Imaginary.Should().BeApproximately(4, 1e-10);
    }

    [Fact]
    public void Modulus_ThreeFourFive_ReturnsFive()
    {
        new ComplexValue(3, 4).Modulus().Should().BeApproximately(5, 1e-10);
    }

    [Fact]
    public void Argument_ImaginaryUnit_ReturnsHalfPi()
    {
        new ComplexValue(0, 1).Argument().Should().BeApproximately(Math.PI / 2, 1e-10);
    }
}
