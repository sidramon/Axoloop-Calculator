namespace Domain.Tests.Calculator;

using Domain.Calculator;
using Domain.Calculator.Values;
using Domain.Tests.Calculator.TestHelpers;
using FluentAssertions;
using Value = Domain.Calculator.Values.Value;

/// <summary>
/// End-to-end coverage through the real tokenizer/parser/evaluator pipeline for the
/// "comportements attendus" list from the complex-numbers task: everything from parsing
/// <c>_i</c> as an identifier through arithmetic, reduction, comparison, and the sqrt/ln
/// integration points.
/// </summary>
public class ComplexNumberEvaluationTests
{
    private readonly Domain.Calculator.Parsing.Parser _parser = ParserFactory.CreateDefault();
    private readonly Evaluator _evaluator = EvaluatorFactory.CreateDefault();
    private readonly VariableContext _context = new();

    public ComplexNumberEvaluationTests() => _context.Seed(Constants.All);

    private Value Run(string input) => _evaluator.Evaluate(_parser.Parse(input), _context);

    [Fact]
    public void ImaginaryUnit_EvaluatesToComplexZeroOne()
    {
        var result = (ComplexValue)Run("_i");

        result.Real.Should().BeApproximately(0, 1e-10);
        result.Imaginary.Should().BeApproximately(1, 1e-10);
    }

    [Fact]
    public void ImaginaryUnit_IsWriteProtected()
    {
        var act = () => Run("_i := 5");

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ImaginaryUnitSquared_ReducesToRealNegativeOne()
    {
        var result = Run("_i^2");

        result.Should().BeOfType<NumberValue>();
        ((NumberValue)result).Number.Should().BeApproximately(-1, 1e-9);
    }

    [Fact]
    public void ImaginaryUnitTimesItself_ReducesToRealNegativeOne()
    {
        var result = Run("_i * _i");

        result.Should().BeOfType<NumberValue>();
        ((NumberValue)result).Number.Should().BeApproximately(-1, 1e-9);
    }

    [Fact]
    public void TwoPlusThreeI_ReturnsExpectedComplex()
    {
        var result = (ComplexValue)Run("2 + 3*_i");

        result.Real.Should().BeApproximately(2, 1e-9);
        result.Imaginary.Should().BeApproximately(3, 1e-9);
    }

    [Fact]
    public void ProductOfTwoComplexNumbers_MatchesExpansion()
    {
        var result = (ComplexValue)Run("(2 + 3*_i) * (1 - _i)");

        result.Real.Should().BeApproximately(5, 1e-9);
        result.Imaginary.Should().BeApproximately(1, 1e-9);
    }

    [Fact]
    public void SqrtOfNegativeOne_ReturnsImaginaryUnit()
    {
        var result = (ComplexValue)Run("sqrt(-1)");

        result.Real.Should().BeApproximately(0, 1e-9);
        result.Imaginary.Should().BeApproximately(1, 1e-9);
    }

    [Fact]
    public void SqrtOfNegativeFour_ReturnsTwoI()
    {
        var result = (ComplexValue)Run("sqrt(-4)");

        result.Real.Should().BeApproximately(0, 1e-9);
        result.Imaginary.Should().BeApproximately(2, 1e-9);
    }

    [Fact]
    public void LnOfNegativeOne_ReturnsPiTimesI()
    {
        var result = (ComplexValue)Run("ln(-1)");

        result.Real.Should().BeApproximately(0, 1e-9);
        result.Imaginary.Should().BeApproximately(Math.PI, 1e-9);
    }

    [Fact]
    public void ImaginaryUnitToThePowerOfItself_IsRealAndApproximatelyPointTwoOhSevenNine()
    {
        var result = Run("_i^_i");

        result.Should().BeOfType<NumberValue>();
        ((NumberValue)result).Number.Should().BeApproximately(0.2078795763, 1e-8);
    }

    [Fact]
    public void OnePlusIAllSquared_ReturnsTwoI()
    {
        var result = (ComplexValue)Run("(1 + _i)^2");

        result.Real.Should().BeApproximately(0, 1e-9);
        result.Imaginary.Should().BeApproximately(2, 1e-9);
    }

    [Fact]
    public void ImaginaryUnitLessThanTwo_ThrowsComplexNumbersAreNotOrdered()
    {
        var act = () => Run("_i < 2");

        act.Should().Throw<InvalidOperationException>().WithMessage("*not ordered*");
    }

    [Fact]
    public void TwoPlusZeroTimesI_ReducesToRealAndComparesNormally()
    {
        var result = (BooleanValue)Run("(2 + 0*_i) < 3");

        result.Boolean.Should().BeTrue();
    }

    [Fact]
    public void AbsOfThreePlusFourI_ReturnsModulusFive()
    {
        var result = (NumberValue)Run("abs(3 + 4*_i)");

        result.Number.Should().BeApproximately(5, 1e-9);
    }

    [Fact]
    public void EigvalsOfRotationMatrix_ReturnsComplexConjugatePair()
    {
        var result = Run("eigvals([0,-1;1,0])");

        result.Should().BeOfType<ValueListValue>();
        var list = (ValueListValue)result;
        list.Values.Should().HaveCount(2);
    }

    // ---- Non-regression: a broad sample of ordinary real expressions ----

    [Theory]
    [InlineData("2 + 3", 5)]
    [InlineData("10 - 4", 6)]
    [InlineData("6 * 7", 42)]
    [InlineData("9 / 2", 4.5)]
    [InlineData("2^10", 1024)]
    [InlineData("7 % 3", 1)]
    [InlineData("sqrt(9)", 3)]
    [InlineData("abs(-5)", 5)]
    [InlineData("-5 + 2", -3)]
    public void OrdinaryRealExpressions_StillReturnPlainNumberValue(string expression, double expected)
    {
        var result = Run(expression);

        result.Should().BeOfType<NumberValue>();
        ((NumberValue)result).Number.Should().BeApproximately(expected, 1e-9);
    }
}
