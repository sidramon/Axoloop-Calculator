namespace Domain.Tests.Calculator.Symbolic;

using System.Numerics;
using Domain.Calculator.Symbolic;
using FluentAssertions;

public class RationalTests
{
    [Fact]
    public void Construct_UnreducedFraction_ReducesAutomatically()
    {
        var r = new Rational(4, 8);

        r.Numerator.Should().Be(1);
        r.Denominator.Should().Be(2);
    }

    [Fact]
    public void Construct_NegativeDenominator_MovesSignToNumerator()
    {
        var r = new Rational(3, -4);

        r.Numerator.Should().Be(-3);
        r.Denominator.Should().Be(4);
    }

    [Fact]
    public void Construct_BothNegative_NormalizesToPositive()
    {
        var r = new Rational(-3, -4);

        r.Numerator.Should().Be(3);
        r.Denominator.Should().Be(4);
    }

    [Fact]
    public void Construct_ZeroNumerator_NormalizesDenominatorToOne()
    {
        var r = new Rational(0, 5);

        r.Denominator.Should().Be(1);
    }

    [Fact]
    public void Construct_ZeroDenominator_Throws()
    {
        var act = () => new Rational(1, 0);

        act.Should().Throw<DivideByZeroException>();
    }

    [Fact]
    public void Addition_HalfPlusThird_ReturnsFiveSixths()
    {
        var a = new Rational(1, 2);
        var b = new Rational(1, 3);

        var result = a + b;

        result.Numerator.Should().Be(5);
        result.Denominator.Should().Be(6);
    }

    [Fact]
    public void Subtraction_HalfMinusThird_ReturnsOneSixth()
    {
        var a = new Rational(1, 2);
        var b = new Rational(1, 3);

        var result = a - b;

        result.Numerator.Should().Be(1);
        result.Denominator.Should().Be(6);
    }

    [Fact]
    public void Multiplication_TwoThirdsTimesThreeFourths_ReturnsOneHalf()
    {
        var a = new Rational(2, 3);
        var b = new Rational(3, 4);

        var result = a * b;

        result.Numerator.Should().Be(1);
        result.Denominator.Should().Be(2);
    }

    [Fact]
    public void Division_HalfDividedByThird_ReturnsThreeHalves()
    {
        var a = new Rational(1, 2);
        var b = new Rational(1, 3);

        var result = a / b;

        result.Numerator.Should().Be(3);
        result.Denominator.Should().Be(2);
    }

    [Fact]
    public void Division_ByZero_Throws()
    {
        var a = new Rational(1, 2);

        var act = () => a / Rational.Zero;

        act.Should().Throw<DivideByZeroException>();
    }

    [Fact]
    public void Negate_PositiveRational_FlipsSign()
    {
        var a = new Rational(3, 4);

        (-a).Numerator.Should().Be(-3);
    }

    [Fact]
    public void Abs_NegativeRational_ReturnsPositive()
    {
        var a = new Rational(-3, 4);

        a.Abs().Numerator.Should().Be(3);
    }

    [Fact]
    public void Pow_PositiveExponent_RaisesNumeratorAndDenominator()
    {
        var a = new Rational(2, 3);

        var result = a.Pow(3);

        result.Numerator.Should().Be(8);
        result.Denominator.Should().Be(27);
    }

    [Fact]
    public void Pow_NegativeExponent_InvertsThenRaises()
    {
        var a = new Rational(2, 3);

        var result = a.Pow(-2);

        result.Numerator.Should().Be(9);
        result.Denominator.Should().Be(4);
    }

    [Fact]
    public void Pow_ZeroExponentOnNonZero_ReturnsOne()
    {
        var a = new Rational(5, 7);

        a.Pow(0).Should().Be(Rational.One);
    }

    [Fact]
    public void Pow_ZeroExponentOnZero_ReturnsOneByConvention()
    {
        // 0^0 = 1: the usual CAS/combinatorial convention, documented on Rational.Pow.
        Rational.Zero.Pow(0).Should().Be(Rational.One);
    }

    [Fact]
    public void Pow_ZeroBaseNegativeExponent_Throws()
    {
        var act = () => Rational.Zero.Pow(-1);

        act.Should().Throw<DivideByZeroException>();
    }

    [Fact]
    public void Comparison_OrdersAcrossDifferentDenominators()
    {
        var a = new Rational(1, 3);
        var b = new Rational(1, 2);

        (a < b).Should().BeTrue();
        (b > a).Should().BeTrue();
    }

    [Fact]
    public void Equality_ReducedEquivalentFractions_AreEqual()
    {
        var a = new Rational(2, 4);
        var b = new Rational(1, 2);

        (a == b).Should().BeTrue();
        a.Equals(b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void LargeNumbers_NoOverflow_ExactArithmeticHolds()
    {
        var huge = new Rational(BigInteger.Pow(10, 30), 1);
        var result = huge * huge;

        result.Numerator.Should().Be(BigInteger.Pow(10, 60));
    }

    [Fact]
    public void ImplicitConversion_FromInt_CreatesIntegerRational()
    {
        Rational r = 5;

        r.Numerator.Should().Be(5);
        r.Denominator.Should().Be(1);
        r.IsInteger.Should().BeTrue();
    }

    [Fact]
    public void ToDouble_RoundTripsApproximately()
    {
        var r = new Rational(1, 3);

        r.ToDouble().Should().BeApproximately(1.0 / 3.0, 1e-12);
    }

    [Fact]
    public void FromDouble_OneTenth_ReturnsExactTenth()
    {
        var r = Rational.FromDouble(0.1);

        r.Numerator.Should().Be(1);
        r.Denominator.Should().Be(10);
    }

    [Fact]
    public void FromDouble_OneHalf_ReturnsExactHalf()
    {
        var r = Rational.FromDouble(0.5);

        r.Numerator.Should().Be(1);
        r.Denominator.Should().Be(2);
    }

    [Fact]
    public void FromDouble_OneThird_ReturnsExactThird()
    {
        var r = Rational.FromDouble(1.0 / 3.0);

        r.Numerator.Should().Be(1);
        r.Denominator.Should().Be(3);
    }

    [Fact]
    public void FromDouble_NegativeValue_PreservesSign()
    {
        var r = Rational.FromDouble(-0.25);

        r.Numerator.Should().Be(-1);
        r.Denominator.Should().Be(4);
    }

    [Fact]
    public void FromDouble_Zero_ReturnsZero()
    {
        Rational.FromDouble(0.0).Should().Be(Rational.Zero);
    }

    [Fact]
    public void FromDouble_Integer_ReturnsExactInteger()
    {
        var r = Rational.FromDouble(7.0);

        r.Numerator.Should().Be(7);
        r.Denominator.Should().Be(1);
    }

    [Fact]
    public void FromDouble_NaN_Throws()
    {
        var act = () => Rational.FromDouble(double.NaN);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void FromDouble_Infinity_Throws()
    {
        var act = () => Rational.FromDouble(double.PositiveInfinity);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ToString_Integer_OmitsDenominator()
    {
        new Rational(5).ToString().Should().Be("5");
    }

    [Fact]
    public void ToString_Fraction_ShowsNumeratorSlashDenominator()
    {
        new Rational(2, 3).ToString().Should().Be("2/3");
    }
}
