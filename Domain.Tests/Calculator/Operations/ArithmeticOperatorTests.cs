namespace Domain.Tests.Calculator.Operations;

using Domain.Calculator.Operations;
using Domain.Calculator.Values;
using FluentAssertions;

public class ArithmeticOperatorTests
{
    // ---- Add ----

    [Fact]
    public void Add_TwoNumbers_ReturnsSum()
    {
        var result = (NumberValue)new AddOperator().Apply(new NumberValue(2), new NumberValue(3));

        result.Number.Should().BeApproximately(5, 1e-10);
    }

    [Fact]
    public void Add_TwoMatrices_ReturnsElementwiseSum()
    {
        var a = new MatrixValue(new double[,] { { 1, 2 } });
        var b = new MatrixValue(new double[,] { { 3, 4 } });

        var result = (MatrixValue)new AddOperator().Apply(a, b);

        result[0, 0].Should().BeApproximately(4, 1e-10);
        result[0, 1].Should().BeApproximately(6, 1e-10);
    }

    [Fact]
    public void Add_NumberAndMatrix_ThrowsInvalidOperationExceptionWithMessage()
    {
        var act = () => new AddOperator().Apply(new NumberValue(1), new MatrixValue(new double[,] { { 1 } }));

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*addition*number*matrix*");
    }

    [Fact]
    public void Add_NumberAndSolutionValue_ThrowsInvalidOperationExceptionNamingSolution()
    {
        var solution = new SolutionValue("x", new[] { 2.5 }, 1);

        var act = () => new AddOperator().Apply(solution, new NumberValue(1));

        act.Should().Throw<InvalidOperationException>().WithMessage("*addition*solution*");
    }

    // ---- Subtract ----

    [Fact]
    public void Subtract_TwoNumbers_ReturnsDifference()
    {
        var result = (NumberValue)new SubtractOperator().Apply(new NumberValue(5), new NumberValue(3));

        result.Number.Should().BeApproximately(2, 1e-10);
    }

    [Fact]
    public void Subtract_IncompatibleTypes_ThrowsInvalidOperationException()
    {
        var act = () => new SubtractOperator().Apply(new BooleanValue(true), new NumberValue(1));

        act.Should().Throw<InvalidOperationException>();
    }

    // ---- Multiply ----

    [Fact]
    public void Multiply_TwoNumbers_ReturnsProduct()
    {
        var result = (NumberValue)new MultiplyOperator().Apply(new NumberValue(4), new NumberValue(5));

        result.Number.Should().BeApproximately(20, 1e-10);
    }

    [Fact]
    public void Multiply_MatrixThenScalar_ScalesEveryElement()
    {
        var matrix = new MatrixValue(new double[,] { { 1, 2 } });

        var result = (MatrixValue)new MultiplyOperator().Apply(matrix, new NumberValue(3));

        result[0, 0].Should().BeApproximately(3, 1e-10);
        result[0, 1].Should().BeApproximately(6, 1e-10);
    }

    [Fact]
    public void Multiply_ScalarThenMatrix_ScalesEveryElement()
    {
        var matrix = new MatrixValue(new double[,] { { 1, 2 } });

        var result = (MatrixValue)new MultiplyOperator().Apply(new NumberValue(3), matrix);

        result[0, 0].Should().BeApproximately(3, 1e-10);
        result[0, 1].Should().BeApproximately(6, 1e-10);
    }

    [Fact]
    public void Multiply_TwoMatrices_ReturnsMatrixProduct()
    {
        var a = new MatrixValue(new double[,] { { 1, 2 } });
        var b = new MatrixValue(new double[,] { { 1 }, { 1 } });

        var result = (MatrixValue)new MultiplyOperator().Apply(a, b);

        result[0, 0].Should().BeApproximately(3, 1e-10);
    }

    [Fact]
    public void Multiply_IncompatibleTypes_ThrowsInvalidOperationException()
    {
        var act = () => new MultiplyOperator().Apply(new BooleanValue(true), new NumberValue(1));

        act.Should().Throw<InvalidOperationException>();
    }

    // ---- Divide ----

    [Fact]
    public void Divide_TwoNumbers_ReturnsQuotient()
    {
        var result = (NumberValue)new DivideOperator().Apply(new NumberValue(10), new NumberValue(4));

        result.Number.Should().BeApproximately(2.5, 1e-10);
    }

    [Fact]
    public void Divide_MatrixByScalar_ScalesEveryElement()
    {
        var matrix = new MatrixValue(new double[,] { { 4, 8 } });

        var result = (MatrixValue)new DivideOperator().Apply(matrix, new NumberValue(4));

        result[0, 0].Should().BeApproximately(1, 1e-10);
        result[0, 1].Should().BeApproximately(2, 1e-10);
    }

    [Fact]
    public void Divide_NumberByZero_ThrowsDivideByZeroException()
    {
        var act = () => new DivideOperator().Apply(new NumberValue(1), new NumberValue(0));

        act.Should().Throw<DivideByZeroException>();
    }

    [Fact]
    public void Divide_MatrixByZero_ThrowsDivideByZeroException()
    {
        var matrix = new MatrixValue(new double[,] { { 1 } });

        var act = () => new DivideOperator().Apply(matrix, new NumberValue(0));

        act.Should().Throw<DivideByZeroException>();
    }

    [Fact]
    public void Divide_IncompatibleTypes_ThrowsInvalidOperationException()
    {
        var act = () => new DivideOperator().Apply(new BooleanValue(true), new NumberValue(1));

        act.Should().Throw<InvalidOperationException>();
    }

    // ---- Modulo ----

    [Fact]
    public void Modulo_TwoNumbers_ReturnsRemainder()
    {
        var result = (NumberValue)new ModuloOperator().Apply(new NumberValue(7), new NumberValue(3));

        result.Number.Should().BeApproximately(1, 1e-10);
    }

    [Fact]
    public void Modulo_ByZero_ThrowsDivideByZeroException()
    {
        var act = () => new ModuloOperator().Apply(new NumberValue(7), new NumberValue(0));

        act.Should().Throw<DivideByZeroException>();
    }

    [Fact]
    public void Modulo_NonNumericTypes_ThrowsInvalidOperationException()
    {
        var matrix = new MatrixValue(new double[,] { { 1 } });

        var act = () => new ModuloOperator().Apply(matrix, new NumberValue(3));

        act.Should().Throw<InvalidOperationException>();
    }

    // ---- Power ----

    [Fact]
    public void Power_TwoNumbers_ReturnsExponentiation()
    {
        var result = (NumberValue)new PowerOperator().Apply(new NumberValue(2), new NumberValue(10));

        result.Number.Should().BeApproximately(1024, 1e-10);
    }

    [Fact]
    public void Power_NonNumericTypes_ThrowsInvalidOperationException()
    {
        var matrix = new MatrixValue(new double[,] { { 1 } });

        var act = () => new PowerOperator().Apply(matrix, new NumberValue(2));

        act.Should().Throw<InvalidOperationException>();
    }
}
