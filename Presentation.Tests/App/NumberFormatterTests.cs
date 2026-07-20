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
}
