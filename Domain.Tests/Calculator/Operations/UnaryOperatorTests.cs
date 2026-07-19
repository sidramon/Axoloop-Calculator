namespace Domain.Tests.Calculator.Operations;

using Domain.Calculator.Operations;
using Domain.Calculator.Values;
using FluentAssertions;

public class UnaryOperatorTests
{
    // ---- Negate ----

    [Fact]
    public void Negate_Number_ReturnsOppositeSign()
    {
        var result = (NumberValue)new NegateOperator().Apply(new NumberValue(5));

        result.Number.Should().BeApproximately(-5, 1e-10);
    }

    [Fact]
    public void Negate_Matrix_ScalesEveryElementByMinusOne()
    {
        var matrix = new MatrixValue(new double[,] { { 1, -2 } });

        var result = (MatrixValue)new NegateOperator().Apply(matrix);

        result[0, 0].Should().BeApproximately(-1, 1e-10);
        result[0, 1].Should().BeApproximately(2, 1e-10);
    }

    [Fact]
    public void Negate_Boolean_ThrowsInvalidOperationException()
    {
        var act = () => new NegateOperator().Apply(new BooleanValue(true));

        act.Should().Throw<InvalidOperationException>();
    }

    // ---- Factorial ----

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 1)]
    [InlineData(5, 120)]
    public void Factorial_NonNegativeInteger_ReturnsProduct(double input, double expected)
    {
        var result = (NumberValue)new FactorialOperator().Apply(new NumberValue(input));

        result.Number.Should().BeApproximately(expected, 1e-10);
    }

    [Fact]
    public void Factorial_NegativeNumber_ThrowsInvalidOperationException()
    {
        var act = () => new FactorialOperator().Apply(new NumberValue(-1));

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Factorial_NonIntegerNumber_ThrowsInvalidOperationException()
    {
        var act = () => new FactorialOperator().Apply(new NumberValue(2.5));

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Factorial_NonNumericOperand_ThrowsInvalidOperationException()
    {
        var act = () => new FactorialOperator().Apply(new BooleanValue(true));

        act.Should().Throw<InvalidOperationException>();
    }
}
