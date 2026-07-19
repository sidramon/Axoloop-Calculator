namespace Domain.Tests.Calculator.Operations.Functions;

using Domain.Calculator.Operations.Functions.Scalar;
using Domain.Calculator.Values;
using FluentAssertions;
using Value = Domain.Calculator.Values.Value;

public class ScalarFunctionTests
{
    // ---- Sqrt ----

    [Fact]
    public void Sqrt_PositiveNumber_ReturnsSquareRoot()
    {
        var result = (NumberValue)new SqrtFunction().Apply(new Value[] { new NumberValue(9) });

        result.Number.Should().BeApproximately(3, 1e-10);
    }

    [Fact]
    public void Sqrt_NegativeNumber_Throws()
    {
        var act = () => new SqrtFunction().Apply(new Value[] { new NumberValue(-1) });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Sqrt_NonNumericArgument_Throws()
    {
        var act = () => new SqrtFunction().Apply(new Value[] { new BooleanValue(true) });

        act.Should().Throw<InvalidOperationException>();
    }

    // ---- NthRoot ----

    [Fact]
    public void NthRoot_PositiveRadicand_ReturnsRoot()
    {
        var result = (NumberValue)new NthRootFunction().Apply(new Value[] { new NumberValue(8), new NumberValue(3) });

        result.Number.Should().BeApproximately(2, 1e-10);
    }

    [Fact]
    public void NthRoot_NegativeRadicandWithOddDegree_ReturnsNegativeRoot()
    {
        var result = (NumberValue)new NthRootFunction().Apply(new Value[] { new NumberValue(-8), new NumberValue(3) });

        result.Number.Should().BeApproximately(-2, 1e-10);
    }

    [Fact]
    public void NthRoot_NegativeRadicandWithEvenDegree_Throws()
    {
        var act = () => new NthRootFunction().Apply(new Value[] { new NumberValue(-8), new NumberValue(2) });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void NthRoot_ZeroDegree_Throws()
    {
        var act = () => new NthRootFunction().Apply(new Value[] { new NumberValue(8), new NumberValue(0) });

        act.Should().Throw<InvalidOperationException>();
    }

    // ---- Pow ----

    [Fact]
    public void Pow_BaseAndExponent_ReturnsPower()
    {
        var result = (NumberValue)new PowFunction().Apply(new Value[] { new NumberValue(2), new NumberValue(10) });

        result.Number.Should().BeApproximately(1024, 1e-10);
    }

    [Fact]
    public void Pow_NonNumericArgument_Throws()
    {
        var act = () => new PowFunction().Apply(new Value[] { new BooleanValue(true), new NumberValue(2) });

        act.Should().Throw<InvalidOperationException>();
    }

    // ---- Ln ----

    [Fact]
    public void Ln_PositiveNumber_ReturnsNaturalLog()
    {
        var result = (NumberValue)new LnFunction().Apply(new Value[] { new NumberValue(Math.E) });

        result.Number.Should().BeApproximately(1, 1e-10);
    }

    [Fact]
    public void Ln_Zero_Throws()
    {
        var act = () => new LnFunction().Apply(new Value[] { new NumberValue(0) });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Ln_NegativeNumber_Throws()
    {
        var act = () => new LnFunction().Apply(new Value[] { new NumberValue(-1) });

        act.Should().Throw<InvalidOperationException>();
    }

    // ---- Log ----

    [Fact]
    public void Log_ValidBaseAndArgument_ReturnsLogarithm()
    {
        var result = (NumberValue)new LogFunction().Apply(new Value[] { new NumberValue(8), new NumberValue(2) });

        result.Number.Should().BeApproximately(3, 1e-10);
    }

    [Fact]
    public void Log_NonPositiveArgument_Throws()
    {
        var act = () => new LogFunction().Apply(new Value[] { new NumberValue(0), new NumberValue(2) });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Log_BaseOfOne_Throws()
    {
        var act = () => new LogFunction().Apply(new Value[] { new NumberValue(8), new NumberValue(1) });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Log_NonPositiveBase_Throws()
    {
        var act = () => new LogFunction().Apply(new Value[] { new NumberValue(8), new NumberValue(-2) });

        act.Should().Throw<InvalidOperationException>();
    }

    // ---- Abs ----

    [Theory]
    [InlineData(-5, 5)]
    [InlineData(5, 5)]
    [InlineData(0, 0)]
    public void Abs_AnyNumber_ReturnsAbsoluteValue(double input, double expected)
    {
        var result = (NumberValue)new AbsFunction().Apply(new Value[] { new NumberValue(input) });

        result.Number.Should().BeApproximately(expected, 1e-10);
    }

    [Fact]
    public void Abs_NonNumericArgument_Throws()
    {
        var act = () => new AbsFunction().Apply(new Value[] { new BooleanValue(false) });

        act.Should().Throw<InvalidOperationException>();
    }
}
