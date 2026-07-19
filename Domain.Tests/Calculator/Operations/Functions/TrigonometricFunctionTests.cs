namespace Domain.Tests.Calculator.Operations.Functions;

using Domain.Calculator.Operations.Functions.Scalar.Trigonometric;
using Domain.Calculator.Values;
using FluentAssertions;
using Value = Domain.Calculator.Values.Value;

public class TrigonometricFunctionTests
{
    private static NumberValue N(double x) => new(x);

    // ---- Direct trig functions ----

    [Theory]
    [InlineData(0, 0)]
    public void Sin_KnownValue_ReturnsExpected(double input, double expected)
    {
        var result = (NumberValue)new SinFunction().Apply(new Value[] { N(input) });

        result.Number.Should().BeApproximately(expected, 1e-10);
    }

    [Theory]
    [InlineData(0, 1)]
    public void Cos_KnownValue_ReturnsExpected(double input, double expected)
    {
        var result = (NumberValue)new CosFunction().Apply(new Value[] { N(input) });

        result.Number.Should().BeApproximately(expected, 1e-10);
    }

    [Fact]
    public void Tan_KnownValue_ReturnsExpected()
    {
        var result = (NumberValue)new TanFunction().Apply(new Value[] { N(0) });

        result.Number.Should().BeApproximately(0, 1e-10);
    }

    [Fact]
    public void Sin_NonNumericArgument_Throws()
    {
        var act = () => new SinFunction().Apply(new Value[] { new BooleanValue(true) });

        act.Should().Throw<InvalidOperationException>();
    }

    // ---- Inverse trig functions ----

    [Fact]
    public void Asin_KnownValue_ReturnsExpected()
    {
        var result = (NumberValue)new AsinFunction().Apply(new Value[] { N(1) });

        result.Number.Should().BeApproximately(Math.PI / 2, 1e-10);
    }

    [Fact]
    public void Asin_OutOfDomain_Throws()
    {
        var act = () => new AsinFunction().Apply(new Value[] { N(2) });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Acos_KnownValue_ReturnsExpected()
    {
        var result = (NumberValue)new AcosFunction().Apply(new Value[] { N(1) });

        result.Number.Should().BeApproximately(0, 1e-10);
    }

    [Fact]
    public void Acos_OutOfDomain_Throws()
    {
        var act = () => new AcosFunction().Apply(new Value[] { N(-2) });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Atan_KnownValue_ReturnsExpected()
    {
        var result = (NumberValue)new AtanFunction().Apply(new Value[] { N(1) });

        result.Number.Should().BeApproximately(Math.PI / 4, 1e-10);
    }

    [Fact]
    public void Atan2_KnownValue_ReturnsExpected()
    {
        var result = (NumberValue)new Atan2Function().Apply(new Value[] { N(1), N(1) });

        result.Number.Should().BeApproximately(Math.PI / 4, 1e-10);
    }

    // ---- Reciprocal trig functions ----

    [Fact]
    public void Csc_KnownValue_ReturnsReciprocalOfSin()
    {
        var result = (NumberValue)new CscFunction().Apply(new Value[] { N(Math.PI / 2) });

        result.Number.Should().BeApproximately(1, 1e-10);
    }

    [Fact]
    public void Csc_AtMultipleOfPi_ThrowsPoleException()
    {
        var act = () => new CscFunction().Apply(new Value[] { N(Math.PI) });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Sec_KnownValue_ReturnsReciprocalOfCos()
    {
        var result = (NumberValue)new SecFunction().Apply(new Value[] { N(0) });

        result.Number.Should().BeApproximately(1, 1e-10);
    }

    [Fact]
    public void Sec_AtOddMultipleOfHalfPi_ThrowsPoleException()
    {
        var act = () => new SecFunction().Apply(new Value[] { N(Math.PI / 2) });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Cot_KnownValue_ReturnsReciprocalOfTan()
    {
        var result = (NumberValue)new CotFunction().Apply(new Value[] { N(Math.PI / 2) });

        result.Number.Should().BeApproximately(0, 1e-10);
    }

    [Fact]
    public void Cot_AtMultipleOfPi_ThrowsPoleException()
    {
        var act = () => new CotFunction().Apply(new Value[] { N(Math.PI) });

        act.Should().Throw<InvalidOperationException>();
    }

    // ---- Inverse reciprocal trig functions ----

    [Fact]
    public void Acsc_KnownValue_ReturnsExpected()
    {
        var result = (NumberValue)new AcscFunction().Apply(new Value[] { N(1) });

        result.Number.Should().BeApproximately(Math.PI / 2, 1e-10);
    }

    [Fact]
    public void Acsc_OutOfDomain_Throws()
    {
        var act = () => new AcscFunction().Apply(new Value[] { N(0.5) });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Asec_KnownValue_ReturnsExpected()
    {
        var result = (NumberValue)new AsecFunction().Apply(new Value[] { N(1) });

        result.Number.Should().BeApproximately(0, 1e-10);
    }

    [Fact]
    public void Asec_OutOfDomain_Throws()
    {
        var act = () => new AsecFunction().Apply(new Value[] { N(0.5) });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Acot_KnownValue_ReturnsExpected()
    {
        var result = (NumberValue)new AcotFunction().Apply(new Value[] { N(0) });

        result.Number.Should().BeApproximately(Math.PI / 2, 1e-10);
    }

    [Fact]
    public void Acot_NonNumericArgument_Throws()
    {
        var act = () => new AcotFunction().Apply(new Value[] { new BooleanValue(true) });

        act.Should().Throw<InvalidOperationException>();
    }
}
