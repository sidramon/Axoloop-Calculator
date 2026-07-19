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
}
