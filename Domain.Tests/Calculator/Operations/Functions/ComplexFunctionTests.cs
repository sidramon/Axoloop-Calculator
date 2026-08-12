namespace Domain.Tests.Calculator.Operations.Functions;

using Domain.Calculator.Operations.Functions.Complex;
using Domain.Calculator.Values;
using FluentAssertions;
using Value = Domain.Calculator.Values.Value;

public class ComplexFunctionTests
{
    // ---- real ----

    [Fact]
    public void Real_ComplexNumber_ReturnsRealPart()
    {
        var result = (NumberValue)new RealPartFunction().Apply(new Value[] { new ComplexValue(3, 2) });

        result.Number.Should().BeApproximately(3, 1e-10);
    }

    [Fact]
    public void Real_PlainReal_ReturnsItself()
    {
        var result = (NumberValue)new RealPartFunction().Apply(new Value[] { new NumberValue(5) });

        result.Number.Should().BeApproximately(5, 1e-10);
    }

    [Fact]
    public void Real_NonNumericArgument_Throws()
    {
        var act = () => new RealPartFunction().Apply(new Value[] { new BooleanValue(true) });

        act.Should().Throw<InvalidOperationException>();
    }

    // ---- imag ----

    [Fact]
    public void Imag_ComplexNumber_ReturnsImaginaryPart()
    {
        var result = (NumberValue)new ImaginaryPartFunction().Apply(new Value[] { new ComplexValue(3, 2) });

        result.Number.Should().BeApproximately(2, 1e-10);
    }

    [Fact]
    public void Imag_PlainReal_ReturnsZero()
    {
        var result = (NumberValue)new ImaginaryPartFunction().Apply(new Value[] { new NumberValue(5) });

        result.Number.Should().BeApproximately(0, 1e-10);
    }

    [Fact]
    public void Imag_NonNumericArgument_Throws()
    {
        var act = () => new ImaginaryPartFunction().Apply(new Value[] { new BooleanValue(true) });

        act.Should().Throw<InvalidOperationException>();
    }

    // ---- conj ----

    [Fact]
    public void Conj_ComplexNumber_NegatesImaginaryPart()
    {
        var result = (ComplexValue)new ConjugateFunction().Apply(new Value[] { new ComplexValue(3, 2) });

        result.Real.Should().BeApproximately(3, 1e-10);
        result.Imaginary.Should().BeApproximately(-2, 1e-10);
    }

    [Fact]
    public void Conj_PlainReal_ReturnsRealUnchangedAsNumberValue()
    {
        // The zero imaginary part must reduce back to a NumberValue, not ComplexValue(5, -0).
        var result = new ConjugateFunction().Apply(new Value[] { new NumberValue(5) });

        result.Should().BeOfType<NumberValue>();
        ((NumberValue)result).Number.Should().BeApproximately(5, 1e-10);
    }

    [Fact]
    public void Conj_NonNumericArgument_Throws()
    {
        var act = () => new ConjugateFunction().Apply(new Value[] { new BooleanValue(true) });

        act.Should().Throw<InvalidOperationException>();
    }

    // ---- arg ----

    [Fact]
    public void Arg_PositiveImaginaryUnit_ReturnsHalfPi()
    {
        var result = (NumberValue)new ArgumentFunction().Apply(new Value[] { new ComplexValue(0, 1) });

        result.Number.Should().BeApproximately(Math.PI / 2, 1e-10);
    }

    [Fact]
    public void Arg_NegativeReal_ReturnsPi()
    {
        var result = (NumberValue)new ArgumentFunction().Apply(new Value[] { new NumberValue(-1) });

        result.Number.Should().BeApproximately(Math.PI, 1e-10);
    }

    [Fact]
    public void Arg_PositiveReal_ReturnsZero()
    {
        var result = (NumberValue)new ArgumentFunction().Apply(new Value[] { new NumberValue(5) });

        result.Number.Should().BeApproximately(0, 1e-10);
    }

    [Fact]
    public void Arg_NonNumericArgument_Throws()
    {
        var act = () => new ArgumentFunction().Apply(new Value[] { new BooleanValue(true) });

        act.Should().Throw<InvalidOperationException>();
    }
}
