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
}
