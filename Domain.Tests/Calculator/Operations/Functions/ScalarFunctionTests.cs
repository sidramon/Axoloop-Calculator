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

    // ---- Deriv ----

    private static FunctionValue Square() => new(
        "f", 1, "f(x)", args => new NumberValue(Math.Pow(((NumberValue)args[0]).Number, 2)));

    [Fact]
    public void Deriv_SquareFunctionAtThree_ReturnsSix()
    {
        var result = (NumberValue)new DerivFunction().Apply(new Value[] { Square(), new NumberValue(3) });

        result.Number.Should().BeApproximately(6, 1e-4);
    }

    [Fact]
    public void Deriv_FirstArgumentNotAFunction_Throws()
    {
        var act = () => new DerivFunction().Apply(new Value[] { new NumberValue(1), new NumberValue(3) });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Deriv_FunctionWithWrongArity_Throws()
    {
        var twoArg = new FunctionValue("g", 2, "g(a, b)", args => args[0]);

        var act = () => new DerivFunction().Apply(new Value[] { twoArg, new NumberValue(3) });

        act.Should().Throw<InvalidOperationException>();
    }

    // ---- Derivative (arity 1, returns a FunctionValue) ----

    [Fact]
    public void Derivative_OfSquare_ReturnsFunctionValueOfArityOne()
    {
        var result = new DerivativeFunction().Apply(new Value[] { Square() });

        result.Should().BeOfType<FunctionValue>();
        ((FunctionValue)result).Arity.Should().Be(1);
    }

    [Fact]
    public void Derivative_InvokedAtThree_EquivalentToDerivAtThree()
    {
        var derivative = (FunctionValue)new DerivativeFunction().Apply(new Value[] { Square() });
        var viaInvoke = ((NumberValue)derivative.Invoke(new Value[] { new NumberValue(3) })).Number;
        var viaDeriv = ((NumberValue)new DerivFunction().Apply(new Value[] { Square(), new NumberValue(3) })).Number;

        viaInvoke.Should().BeApproximately(viaDeriv, 1e-9);
    }

    [Fact]
    public void Derivative_FirstArgumentNotAFunction_Throws()
    {
        var act = () => new DerivativeFunction().Apply(new Value[] { new NumberValue(1) });

        act.Should().Throw<InvalidOperationException>();
    }

    // ---- NthDerivative (arity 3) ----

    private static FunctionValue Cube() => new(
        "f", 1, "f(x)", args => new NumberValue(Math.Pow(((NumberValue)args[0]).Number, 3)));

    [Fact]
    public void NthDerivative_SecondDerivativeOfCubeAtTwo_ReturnsTwelve()
    {
        var result = (NumberValue)new NthDerivativeFunction().Apply(
            new Value[] { Cube(), new NumberValue(2), new NumberValue(2) });

        result.Number.Should().BeApproximately(12, 1e-5);
    }

    [Fact]
    public void NthDerivative_ThirdDerivativeOfCube_ReturnsSix()
    {
        var result = (NumberValue)new NthDerivativeFunction().Apply(
            new Value[] { Cube(), new NumberValue(3), new NumberValue(2) });

        result.Number.Should().BeApproximately(6, 1e-4);
    }

    [Fact]
    public void NthDerivative_OrderFive_Throws()
    {
        var act = () => new NthDerivativeFunction().Apply(
            new Value[] { Square(), new NumberValue(5), new NumberValue(1) });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void NthDerivative_NonIntegerOrder_Throws()
    {
        var act = () => new NthDerivativeFunction().Apply(
            new Value[] { Square(), new NumberValue(2.5), new NumberValue(1) });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void NthDerivative_NegativeOrder_Throws()
    {
        var act = () => new NthDerivativeFunction().Apply(
            new Value[] { Square(), new NumberValue(-1), new NumberValue(1) });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void NthDerivative_FirstArgumentNotAFunction_Throws()
    {
        var act = () => new NthDerivativeFunction().Apply(
            new Value[] { new NumberValue(1), new NumberValue(2), new NumberValue(1) });

        act.Should().Throw<InvalidOperationException>();
    }

    // ---- Integral ----

    [Fact]
    public void Integral_SquareFunctionOverZeroToOne_ReturnsOneThird()
    {
        var result = (NumberValue)new IntegralFunction().Apply(
            new Value[] { Square(), new NumberValue(0), new NumberValue(1) });

        result.Number.Should().BeApproximately(1.0 / 3.0, 1e-9);
    }

    [Fact]
    public void Integral_FirstArgumentNotAFunction_Throws()
    {
        var act = () => new IntegralFunction().Apply(
            new Value[] { new NumberValue(1), new NumberValue(0), new NumberValue(1) });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Integral_FunctionWithWrongArity_Throws()
    {
        var twoArg = new FunctionValue("g", 2, "g(a, b)", args => args[0]);

        var act = () => new IntegralFunction().Apply(
            new Value[] { twoArg, new NumberValue(0), new NumberValue(1) });

        act.Should().Throw<InvalidOperationException>();
    }

    // ---- Antiderivative (arity 1 and 2, returns a FunctionValue) ----

    [Fact]
    public void Antiderivative_OfSquare_ReturnsFunctionValueOfArityOne()
    {
        var result = new AntiderivativeFunction().Apply(new Value[] { Square() });

        result.Should().BeOfType<FunctionValue>();
        ((FunctionValue)result).Arity.Should().Be(1);
    }

    [Fact]
    public void Antiderivative_NoBasePoint_InvokedAtOne_ReturnsOneThird()
    {
        var primitive = (FunctionValue)new AntiderivativeFunction().Apply(new Value[] { Square() });

        var result = ((NumberValue)primitive.Invoke(new Value[] { new NumberValue(1) })).Number;

        result.Should().BeApproximately(1.0 / 3.0, 1e-6);
    }

    [Fact]
    public void Antiderivative_WithBasePoint_MatchesDefiniteIntegralBetweenBaseAndX()
    {
        var primitive = (FunctionValue)new AntiderivativeFunction(hasExplicitBasePoint: true)
            .Apply(new Value[] { Square(), new NumberValue(1) });
        var viaPrimitive = ((NumberValue)primitive.Invoke(new Value[] { new NumberValue(2) })).Number;

        var viaIntegral = ((NumberValue)new IntegralFunction().Apply(
            new Value[] { Square(), new NumberValue(1), new NumberValue(2) })).Number;

        viaPrimitive.Should().BeApproximately(viaIntegral, 1e-6);
    }

    [Fact]
    public void Antiderivative_FirstArgumentNotAFunction_Throws()
    {
        var act = () => new AntiderivativeFunction().Apply(new Value[] { new NumberValue(1) });

        act.Should().Throw<InvalidOperationException>();
    }

    // ---- Plot ----

    [Fact]
    public void Plot_SquareFunction_ReturnsHundredByTwoMatrixOfSamplePairs()
    {
        var result = (MatrixValue)new PlotFunction().Apply(
            new Value[] { Square(), new NumberValue(-2), new NumberValue(2) });

        result.Rows.Should().Be(100);
        result.Columns.Should().Be(2);
        result[0, 0].Should().BeApproximately(-2, 1e-9);
        result[result.Rows - 1, 0].Should().BeApproximately(2, 1e-9);
        result[0, 1].Should().BeApproximately(4, 1e-9);
    }

    [Fact]
    public void Plot_FirstArgumentNotAFunction_Throws()
    {
        var act = () => new PlotFunction().Apply(
            new Value[] { new NumberValue(1), new NumberValue(-2), new NumberValue(2) });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Plot_FunctionWithWrongArity_Throws()
    {
        var twoArg = new FunctionValue("g", 2, "g(a, b)", args => args[0]);

        var act = () => new PlotFunction().Apply(
            new Value[] { twoArg, new NumberValue(-2), new NumberValue(2) });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Plot_InvalidDomain_Throws()
    {
        var act = () => new PlotFunction().Apply(
            new Value[] { Square(), new NumberValue(2), new NumberValue(-2) });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Plot_FunctionThrowingOnPartOfDomain_RecordsNaNInsteadOfFailing()
    {
        var ln = new FunctionValue("ln", 1, "ln(x)", args =>
        {
            var x = ((NumberValue)args[0]).Number;
            if (x <= 0) throw new InvalidOperationException("domain error");
            return new NumberValue(Math.Log(x));
        });

        var result = (MatrixValue)new PlotFunction().Apply(
            new Value[] { ln, new NumberValue(-1), new NumberValue(1) });

        var hasNaN = false;
        for (var r = 0; r < result.Rows; r++)
            if (double.IsNaN(result[r, 1])) hasNaN = true;

        hasNaN.Should().BeTrue();
    }
}
