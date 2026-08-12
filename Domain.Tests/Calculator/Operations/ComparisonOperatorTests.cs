namespace Domain.Tests.Calculator.Operations;

using Domain.Calculator.Operations;
using Domain.Calculator.Values;
using FluentAssertions;

public class ComparisonOperatorTests
{
    [Theory]
    [InlineData(3, 3, true)]
    [InlineData(3, 4, false)]
    public void Equals_TwoNumbers_ComparesForEquality(double left, double right, bool expected)
    {
        var result = (BooleanValue)new EqualsOperator().Apply(new NumberValue(left), new NumberValue(right));

        result.Boolean.Should().Be(expected);
    }

    [Theory]
    [InlineData(3, 4, true)]
    [InlineData(4, 4, true)]
    [InlineData(5, 4, false)]
    public void LessOrEqual_TwoNumbers_ComparesCorrectly(double left, double right, bool expected)
    {
        var result = (BooleanValue)new LessOrEqualOperator().Apply(new NumberValue(left), new NumberValue(right));

        result.Boolean.Should().Be(expected);
    }

    [Theory]
    [InlineData(5, 4, true)]
    [InlineData(4, 4, true)]
    [InlineData(3, 4, false)]
    public void GreaterOrEqual_TwoNumbers_ComparesCorrectly(double left, double right, bool expected)
    {
        var result = (BooleanValue)new GreaterOrEqualOperator().Apply(new NumberValue(left), new NumberValue(right));

        result.Boolean.Should().Be(expected);
    }

    [Fact]
    public void Equals_NonNumericOperands_ThrowsInvalidOperationException()
    {
        var matrix = new MatrixValue(new double[,] { { 1 } });

        var act = () => new EqualsOperator().Apply(matrix, new NumberValue(1));

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void LessOrEqual_NonNumericOperands_ThrowsInvalidOperationException()
    {
        var act = () => new LessOrEqualOperator().Apply(new BooleanValue(true), new NumberValue(1));

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GreaterOrEqual_NonNumericOperands_ThrowsInvalidOperationException()
    {
        var act = () => new GreaterOrEqualOperator().Apply(new BooleanValue(true), new NumberValue(1));

        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData(3, 5, true)]
    [InlineData(5, 5, false)]
    [InlineData(6, 5, false)]
    public void Less_TwoNumbers_ComparesCorrectly(double left, double right, bool expected)
    {
        var result = (BooleanValue)new LessOperator().Apply(new NumberValue(left), new NumberValue(right));

        result.Boolean.Should().Be(expected);
    }

    [Theory]
    [InlineData(6, 5, true)]
    [InlineData(5, 5, false)]
    [InlineData(3, 5, false)]
    public void Greater_TwoNumbers_ComparesCorrectly(double left, double right, bool expected)
    {
        var result = (BooleanValue)new GreaterOperator().Apply(new NumberValue(left), new NumberValue(right));

        result.Boolean.Should().Be(expected);
    }

    [Fact]
    public void Less_NonNumericOperands_ThrowsInvalidOperationException()
    {
        var act = () => new LessOperator().Apply(new BooleanValue(true), new NumberValue(1));

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Greater_NonNumericOperands_ThrowsInvalidOperationException()
    {
        var act = () => new GreaterOperator().Apply(new BooleanValue(true), new NumberValue(1));

        act.Should().Throw<InvalidOperationException>();
    }

    // ---- Complex equality ----

    [Fact]
    public void Equals_TwoEqualComplexNumbers_ReturnsTrue()
    {
        var result = (BooleanValue)new EqualsOperator().Apply(new ComplexValue(2, 3), new ComplexValue(2, 3));

        result.Boolean.Should().BeTrue();
    }

    [Fact]
    public void Equals_ComplexNumbersDifferingInImaginaryPart_ReturnsFalse()
    {
        var result = (BooleanValue)new EqualsOperator().Apply(new ComplexValue(2, 3), new ComplexValue(2, -3));

        result.Boolean.Should().BeFalse();
    }

    [Fact]
    public void Equals_RealAndComplexWithZeroImaginaryPart_ReturnsTrue()
    {
        var result = (BooleanValue)new EqualsOperator().Apply(new NumberValue(2), new ComplexValue(2, 0));

        result.Boolean.Should().BeTrue();
    }

    [Fact]
    public void Equals_RealAndComplexWithNonZeroImaginaryPart_ReturnsFalse()
    {
        var result = (BooleanValue)new EqualsOperator().Apply(new NumberValue(2), new ComplexValue(2, 1));

        result.Boolean.Should().BeFalse();
    }

    // ---- Complex ordering: rejected outright ----

    [Fact]
    public void Less_ComplexOperand_ThrowsComplexNumbersAreNotOrdered()
    {
        var act = () => new LessOperator().Apply(new ComplexValue(0, 1), new NumberValue(2));

        act.Should().Throw<InvalidOperationException>().WithMessage("*not ordered*");
    }

    [Fact]
    public void Greater_ComplexOperand_ThrowsComplexNumbersAreNotOrdered()
    {
        var act = () => new GreaterOperator().Apply(new NumberValue(2), new ComplexValue(0, 1));

        act.Should().Throw<InvalidOperationException>().WithMessage("*not ordered*");
    }

    [Fact]
    public void LessOrEqual_ComplexOperand_ThrowsComplexNumbersAreNotOrdered()
    {
        var act = () => new LessOrEqualOperator().Apply(new ComplexValue(0, 1), new ComplexValue(1, 0));

        act.Should().Throw<InvalidOperationException>().WithMessage("*not ordered*");
    }

    [Fact]
    public void GreaterOrEqual_ComplexOperand_ThrowsComplexNumbersAreNotOrdered()
    {
        var act = () => new GreaterOrEqualOperator().Apply(new ComplexValue(0, 1), new ComplexValue(1, 0));

        act.Should().Throw<InvalidOperationException>().WithMessage("*not ordered*");
    }

    [Fact]
    public void Less_RawComplexValueWithZeroImaginaryPart_IsOrderedByRealPart()
    {
        // A directly-constructed ComplexValue(2, 0) (bypassing ComplexValue.Of) must still
        // compare correctly rather than being spuriously rejected as "not ordered".
        var result = (BooleanValue)new LessOperator().Apply(new ComplexValue(2, 0), new NumberValue(3));

        result.Boolean.Should().BeTrue();
    }

    [Fact]
    public void Less_ComplexReducedToRealByPriorArithmetic_IsOrderedNormally()
    {
        // (2 + 0*i) reduces to a plain NumberValue before comparison ever sees it — this is
        // the evaluator-level round trip, not a raw ComplexValue(2, 0) reaching the operator.
        var reduced = new AddOperator().Apply(new NumberValue(2), new MultiplyOperator().Apply(
            new NumberValue(0), new ComplexValue(0, 1)));

        var result = (BooleanValue)new LessOperator().Apply(reduced, new NumberValue(3));

        result.Boolean.Should().BeTrue();
    }
}
