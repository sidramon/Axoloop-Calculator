namespace Presentation.Tests.App.Plotting;

using System.Text.RegularExpressions;
using Application.Calculator.Plotting;
using Domain.Calculator.Algorithms;
using Domain.Calculator.Plotting;
using Presentation.App.Plotting;
using FluentAssertions;

public class AsciiPlotRendererTests
{
    private static readonly Regex MarkupTag = new(@"\[[^\]]*\]", RegexOptions.Compiled);

    private static string StripMarkup(string text) => MarkupTag.Replace(text, "");

    [Fact]
    public void Render_ExplicitDimensions_ProducesGridMatchingRequestedWidthAndHeight()
    {
        var points = Enumerable.Range(0, 21).Select(i => new PlotPoint(i - 10, i - 10)).ToList();
        var series = new PlotSeries("f", points, -10, 10, -10, 10, Array.Empty<double>(), Array.Empty<Extremum>());
        var options = new PlotOptions(Width: 40, Height: 20, VisibleXMin: -10, VisibleXMax: 10, ShowZeros: false);

        var rendered = new AsciiPlotRenderer().Render(series, options);
        var lines = rendered.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var gridLines = lines.Skip(1).Where(l => !l.Contains("x:")).Select(StripMarkup).ToList();

        // Height (20) minus the renderer's 3 reserved lines (header, x-axis label, zeros).
        gridLines.Should().HaveCount(17);
        gridLines.Should().OnlyContain(l => l.Length == 40);
    }

    [Fact]
    public void Render_SeriesWithGap_LeavesGapColumnBlankAcrossEveryRow()
    {
        var points = new List<PlotPoint> { new(-2, -2), new(-1, -1), new(0, null), new(1, 1), new(2, 2) };
        var series = new PlotSeries("f", points, -2, 2, -2, 2, Array.Empty<double>(), Array.Empty<Extremum>());
        var options = new PlotOptions(
            Width: 5, Height: 13, VisibleXMin: -2, VisibleXMax: 2, ShowZeros: false, ShowGrid: false);

        var rendered = new AsciiPlotRenderer().Render(series, options);
        var lines = rendered.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var gridLines = lines.Skip(1).Where(l => !l.Contains("x:")).Select(StripMarkup).ToList();

        // Column 2 corresponds to x = 0, the gap: it must stay blank in every row, never
        // connected to the neighboring curve points.
        gridLines.Should().OnlyContain(l => l[2] == ' ');
    }

    [Fact]
    public void Render_ContinuousSeries_DrawsAtLeastOneCurveCharacter()
    {
        var points = Enumerable.Range(0, 21).Select(i => new PlotPoint(i - 10, i - 10)).ToList();
        var series = new PlotSeries("f", points, -10, 10, -10, 10, Array.Empty<double>(), Array.Empty<Extremum>());
        var options = new PlotOptions(Width: 40, Height: 20, VisibleXMin: -10, VisibleXMax: 10, ShowZeros: false);

        var rendered = new AsciiPlotRenderer().Render(series, options);

        rendered.Should().Contain("[cyan]*[/]");
    }

    [Fact]
    public void Render_SeriesWithExtrema_MarksMaximumAndMinimumWithDistinctCharacters()
    {
        var points = Enumerable.Range(0, 21).Select(i => new PlotPoint(i - 10, i - 10)).ToList();
        var extrema = new[]
        {
            new Extremum(-5, -5, ExtremumKind.Minimum),
            new Extremum(5, 5, ExtremumKind.Maximum),
        };
        var series = new PlotSeries("f", points, -10, 10, -10, 10, Array.Empty<double>(), extrema);
        var options = new PlotOptions(Width: 40, Height: 20, VisibleXMin: -10, VisibleXMax: 10, ShowZeros: false);

        var rendered = new AsciiPlotRenderer().Render(series, options);

        rendered.Should().Contain("[green]^[/]");
        rendered.Should().Contain("[red]v[/]");
    }
}
