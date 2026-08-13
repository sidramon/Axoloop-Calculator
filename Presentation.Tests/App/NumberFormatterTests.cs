namespace Presentation.Tests.App;

using Presentation.App;
using FluentAssertions;

public class NumberFormatterTests
{
    [Fact]
    public void FormatNumber_ValueBelowMinValue_SnapsToZero()
    {
        var formatter = new NumberFormatter(FormatOptions.Default);

        formatter.FormatNumber(1e-17).Should().Be("0");
    }

    [Fact]
    public void FormatNumber_NegativeValueBelowMinValue_SnapsToZero()
    {
        var formatter = new NumberFormatter(FormatOptions.Default);

        formatter.FormatNumber(-1e-17).Should().Be("0");
    }

    [Fact]
    public void FormatNumber_ValueAboveMaxValue_SwitchesToScientificNotation()
    {
        var formatter = new NumberFormatter(FormatOptions.Default);

        var result = formatter.FormatNumber(1e20);

        result.Should().Contain("E+");
    }

    [Fact]
    public void FormatNumber_NaN_ReturnsNaNLiteral()
    {
        var formatter = new NumberFormatter(FormatOptions.Default);

        formatter.FormatNumber(double.NaN).Should().Be("NaN");
    }

    [Fact]
    public void FormatNumber_PositiveInfinity_ReturnsInfinityLiteral()
    {
        var formatter = new NumberFormatter(FormatOptions.Default);

        formatter.FormatNumber(double.PositiveInfinity).Should().Be("Infinity");
    }

    [Fact]
    public void FormatNumber_NegativeInfinity_ReturnsNegativeInfinityLiteral()
    {
        var formatter = new NumberFormatter(FormatOptions.Default);

        formatter.FormatNumber(double.NegativeInfinity).Should().Be("-Infinity");
    }

    [Theory]
    [InlineData(2, "0.33")]
    [InlineData(4, "0.3333")]
    public void FormatNumber_ConfigurablePrecision_RoundsToMatchingPattern(int precision, string expected)
    {
        var formatter = new NumberFormatter(new FormatOptions { Precision = precision });

        formatter.FormatNumber(1.0 / 3.0).Should().Be(expected);
    }

    [Fact]
    public void Format_NumberValue_UsesNumberFormatting()
    {
        var formatter = new NumberFormatter(FormatOptions.Default);

        formatter.Format(new Domain.Calculator.Values.NumberValue(2.5)).Should().Be("2.5");
    }

    [Fact]
    public void Format_BooleanValueTrue_ReturnsTrueLiteral()
    {
        var formatter = new NumberFormatter(FormatOptions.Default);

        formatter.Format(new Domain.Calculator.Values.BooleanValue(true)).Should().Be("True");
    }

    [Fact]
    public void Format_BooleanValueFalse_ReturnsFalseLiteral()
    {
        var formatter = new NumberFormatter(FormatOptions.Default);

        formatter.Format(new Domain.Calculator.Values.BooleanValue(false)).Should().Be("False");
    }

    [Fact]
    public void Format_FunctionValue_RendersNameAndParametersWithoutBody()
    {
        var formatter = new NumberFormatter(FormatOptions.Default);
        var function = new Domain.Calculator.Values.FunctionValue(
            "g", 2, "g(a, b)", args => args[0]);

        formatter.Format(function).Should().Be("g(a, b)");
    }

    [Fact]
    public void Format_SolutionValueSingleRoot_RendersUnknownEqualsValue()
    {
        var formatter = new NumberFormatter(FormatOptions.Default);
        var solution = new Domain.Calculator.Values.SolutionValue("x", new[] { 2.5 }, 1);

        formatter.Format(solution).Should().Be("x = 2.5");
    }

    [Fact]
    public void FormatSolutionLines_MultipleRoots_ReturnsOneLinePerRootInOrder()
    {
        var formatter = new NumberFormatter(FormatOptions.Default);
        var solution = new Domain.Calculator.Values.SolutionValue("x", new[] { -2.0, 2.0 }, 2);

        var lines = formatter.FormatSolutionLines(solution);

        lines.Should().Equal("x = -2", "x = 2");
    }

    [Fact]
    public void FormatSolutionHint_TotalFoundMatchesReturnedCount_ReturnsNull()
    {
        var formatter = new NumberFormatter(FormatOptions.Default);
        var solution = new Domain.Calculator.Values.SolutionValue("x", new[] { -2.0, 2.0 }, 2);

        formatter.FormatSolutionHint(solution).Should().BeNull();
    }

    [Fact]
    public void FormatSolutionHint_TotalFoundExceedsReturnedCount_MentionsTotalAndNarrowerDomain()
    {
        var formatter = new NumberFormatter(FormatOptions.Default);
        var solution = new Domain.Calculator.Values.SolutionValue(
            "x", Enumerable.Range(0, 10).Select(i => (double)i).ToList(), 63);

        var hint = formatter.FormatSolutionHint(solution);

        hint.Should().NotBeNull();
        hint.Should().Contain("63");
        hint.Should().Contain("10");
    }

    // ---- Complex formatting ----

    [Fact]
    public void Format_ComplexPositiveImaginary_RendersPlusSign()
    {
        var formatter = new NumberFormatter(FormatOptions.Default);

        formatter.Format(new Domain.Calculator.Values.ComplexValue(3, 2)).Should().Be("3 + 2i");
    }

    [Fact]
    public void Format_ComplexNegativeImaginary_RendersMinusSignNotPlusMinus()
    {
        var formatter = new NumberFormatter(FormatOptions.Default);

        formatter.Format(new Domain.Calculator.Values.ComplexValue(3, -2)).Should().Be("3 - 2i");
    }

    [Fact]
    public void Format_ComplexZeroRealPart_OmitsRealPart()
    {
        var formatter = new NumberFormatter(FormatOptions.Default);

        formatter.Format(new Domain.Calculator.Values.ComplexValue(0, 2)).Should().Be("2i");
    }

    [Fact]
    public void Format_ComplexZeroRealPartNegativeImaginary_OmitsRealPart()
    {
        var formatter = new NumberFormatter(FormatOptions.Default);

        formatter.Format(new Domain.Calculator.Values.ComplexValue(0, -2)).Should().Be("-2i");
    }

    [Fact]
    public void Format_ComplexUnitImaginary_RendersBareI()
    {
        var formatter = new NumberFormatter(FormatOptions.Default);

        formatter.Format(new Domain.Calculator.Values.ComplexValue(0, 1)).Should().Be("i");
    }

    [Fact]
    public void Format_ComplexNegativeUnitImaginary_RendersBareNegativeI()
    {
        var formatter = new NumberFormatter(FormatOptions.Default);

        formatter.Format(new Domain.Calculator.Values.ComplexValue(0, -1)).Should().Be("-i");
    }

    [Fact]
    public void Format_ComplexUnitImaginaryWithRealPart_OmitsMagnitudeOne()
    {
        var formatter = new NumberFormatter(FormatOptions.Default);

        formatter.Format(new Domain.Calculator.Values.ComplexValue(5, 1)).Should().Be("5 + i");
    }

    [Fact]
    public void Format_ComplexNegativeUnitImaginaryWithRealPart_OmitsMagnitudeOne()
    {
        var formatter = new NumberFormatter(FormatOptions.Default);

        formatter.Format(new Domain.Calculator.Values.ComplexValue(5, -1)).Should().Be("5 - i");
    }

    [Fact]
    public void Format_ComplexNegligibleImaginaryPart_FallsBackToRealTextDefensively()
    {
        // Construction always reduces this to a NumberValue in practice — this exercises the
        // formatter's own defensive branch directly, bypassing ComplexValue.Of.
        var formatter = new NumberFormatter(FormatOptions.Default);

        formatter.Format(new Domain.Calculator.Values.ComplexValue(3, 0)).Should().Be("3");
    }

    [Fact]
    public void Format_RealNumberValue_UnaffectedByComplexFormattingPath()
    {
        var formatter = new NumberFormatter(FormatOptions.Default);

        formatter.Format(new Domain.Calculator.Values.NumberValue(-7.5)).Should().Be("-7.5");
    }

    [Fact]
    public void Format_ValueListOfComplexAndReal_RendersBracketedCommaList()
    {
        var formatter = new NumberFormatter(FormatOptions.Default);
        var list = new Domain.Calculator.Values.ValueListValue(new Domain.Calculator.Values.Value[]
        {
            new Domain.Calculator.Values.ComplexValue(0, 1),
            new Domain.Calculator.Values.ComplexValue(0, -1),
        });

        formatter.Format(list).Should().Be("[i, -i]");
    }

    // ---- Limit formatting ----

    [Fact]
    public void Format_LimitConverges_RendersVariableArrowTargetColonValue()
    {
        var formatter = new NumberFormatter(FormatOptions.Default);
        var limit = new Domain.Calculator.Values.LimitValue(
            "x", 0, null, Domain.Calculator.Algorithms.LimitKind.Converges, 1, 1, 1);

        formatter.Format(limit).Should().Be("x → 0 : 1");
    }

    [Fact]
    public void Format_LimitDiverges_RendersPlusInfinity()
    {
        var formatter = new NumberFormatter(FormatOptions.Default);
        var limit = new Domain.Calculator.Values.LimitValue(
            "x", 0, null, Domain.Calculator.Algorithms.LimitKind.DivergesToPositiveInfinity,
            double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity);

        formatter.Format(limit).Should().Be("x → 0 : +∞");
    }

    [Fact]
    public void Format_LimitDivergesNegative_RendersMinusInfinity()
    {
        var formatter = new NumberFormatter(FormatOptions.Default);
        var limit = new Domain.Calculator.Values.LimitValue(
            "x", 0, null, Domain.Calculator.Algorithms.LimitKind.DivergesToNegativeInfinity,
            double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity);

        formatter.Format(limit).Should().Be("x → 0 : -∞");
    }

    [Fact]
    public void Format_LimitOneSidedDiffer_ShowsBothSidesAndSaysTwoSidedLimitDoesNotExist()
    {
        var formatter = new NumberFormatter(FormatOptions.Default);
        var limit = new Domain.Calculator.Values.LimitValue(
            "x", 0, null, Domain.Calculator.Algorithms.LimitKind.OneSidedDiffer,
            null, double.NegativeInfinity, double.PositiveInfinity);

        formatter.Format(limit).Should().Be(
            "x → 0⁻ : -∞, x → 0⁺ : +∞ (two-sided limit does not exist)");
    }

    [Fact]
    public void Format_LimitNoLimit_RendersOscillatesMessageNotAValue()
    {
        var formatter = new NumberFormatter(FormatOptions.Default);
        var limit = new Domain.Calculator.Values.LimitValue(
            "x", 0, null, Domain.Calculator.Algorithms.LimitKind.NoLimit, null, null, null);

        formatter.Format(limit).Should().Be("x → 0 : no limit (oscillates)");
    }

    [Fact]
    public void Format_LimitOneSidedRequestFromTheRight_RendersRightArrowOnly()
    {
        var formatter = new NumberFormatter(FormatOptions.Default);
        var limit = new Domain.Calculator.Values.LimitValue(
            "x", 0, 1, Domain.Calculator.Algorithms.LimitKind.DivergesToPositiveInfinity,
            double.PositiveInfinity, null, null);

        formatter.Format(limit).Should().Be("x → 0⁺ : +∞");
    }

    [Fact]
    public void Format_LimitOneSidedRequestFromTheLeft_RendersLeftArrowOnly()
    {
        var formatter = new NumberFormatter(FormatOptions.Default);
        var limit = new Domain.Calculator.Values.LimitValue(
            "x", 0, -1, Domain.Calculator.Algorithms.LimitKind.DivergesToNegativeInfinity,
            double.NegativeInfinity, null, null);

        formatter.Format(limit).Should().Be("x → 0⁻ : -∞");
    }

    [Fact]
    public void Format_LimitAtPositiveInfinityTarget_RendersInfinitySymbolNotInfinityWord()
    {
        var formatter = new NumberFormatter(FormatOptions.Default);
        var limit = new Domain.Calculator.Values.LimitValue(
            "x", double.PositiveInfinity, null, Domain.Calculator.Algorithms.LimitKind.Converges, 0, 0, 0);

        formatter.Format(limit).Should().Be("x → ∞ : 0");
    }

    [Fact]
    public void Format_LimitAtNegativeInfinityTarget_RendersNegativeInfinitySymbol()
    {
        var formatter = new NumberFormatter(FormatOptions.Default);
        var limit = new Domain.Calculator.Values.LimitValue(
            "x", double.NegativeInfinity, null, Domain.Calculator.Algorithms.LimitKind.Converges, 0, 0, 0);

        formatter.Format(limit).Should().Be("x → -∞ : 0");
    }
}
