namespace Presentation.App.Plotting;

using System.Globalization;
using System.Text;
using Application.Calculator.Plotting;
using Domain.Calculator.Algorithms;
using Domain.Calculator.Plotting;
using Spectre.Console;

/// <summary>
/// Renders a <see cref="PlotSeries"/> as a character grid sized to the terminal
/// (falling back to 80x24 when the console size can't be determined). Lives in
/// Presentation rather than Infrastructure: this is direct terminal output, no
/// external I/O.
/// </summary>
public sealed class AsciiPlotRenderer : IPlotRenderer
{
    private const int FallbackWidth = 80;
    private const int FallbackHeight = 24;
    private const int ReservedRows = 3; // header line, x-axis label line, zeros line

    public PlotFormat Format => PlotFormat.Ascii;

    public string Render(PlotSeries series, PlotOptions options)
    {
        var width = options.Width > 0 ? options.Width : Math.Max(20, DetectConsoleWidth());
        var totalHeight = options.Height > 0
            ? options.Height
            : Math.Max(10 + ReservedRows, DetectConsoleHeight());
        var plotHeight = totalHeight - ReservedRows;

        var xMin = options.VisibleXMin;
        var xMax = options.VisibleXMax;
        var yMin = options.VisibleYMin ?? series.YMin;
        var yMax = options.VisibleYMax ?? series.YMax;

        var cells = new CellKind[plotHeight, width];

        if (options.ShowGrid)
            DrawAxes(cells, xMin, xMax, yMin, yMax, width, plotHeight);

        DrawCurve(cells, series.Points, xMin, xMax, yMin, yMax, width, plotHeight);

        // Best-effort: a coarse grid may not have room to show every extremum distinctly,
        // and that's fine — the marker simply overwrites whatever was in that cell.
        DrawExtrema(cells, series.Extrema, xMin, xMax, yMin, yMax, width, plotHeight);

        var text = new StringBuilder();
        text.Append($"[bold]{Markup.Escape(series.FunctionName)}[/]  ")
            .Append($"[grey]y in [[{FormatNumber(yMin)}, {FormatNumber(yMax)}]][/]\n");

        for (var row = 0; row < plotHeight; row++)
        {
            for (var col = 0; col < width; col++)
                text.Append(RenderCell(cells[row, col]));
            text.Append('\n');
        }

        text.Append($"[grey]x: {FormatNumber(xMin)} .. {FormatNumber(xMax)}[/]\n");

        if (options.ShowZeros)
        {
            var visibleZeros = series.Zeros.Where(z => z >= xMin && z <= xMax).ToList();
            if (visibleZeros.Count > 0)
                text.Append($"[yellow]zeros:[/] {string.Join(", ", visibleZeros.Select(FormatNumber))}\n");

            var visibleMaxima = series.Extrema
                .Where(e => e.Kind == ExtremumKind.Maximum && e.X >= xMin && e.X <= xMax).ToList();
            if (visibleMaxima.Count > 0)
                text.Append($"[green]maxima:[/] {string.Join(", ", visibleMaxima.Select(e => FormatNumber(e.X)))}\n");

            var visibleMinima = series.Extrema
                .Where(e => e.Kind == ExtremumKind.Minimum && e.X >= xMin && e.X <= xMax).ToList();
            if (visibleMinima.Count > 0)
                text.Append($"[red]minima:[/] {string.Join(", ", visibleMinima.Select(e => FormatNumber(e.X)))}\n");
        }

        return text.ToString();
    }

    private enum CellKind { Blank, AxisVertical, AxisHorizontal, Origin, Curve, Maximum, Minimum }

    private static void DrawAxes(
        CellKind[,] cells, double xMin, double xMax, double yMin, double yMax, int width, int height)
    {
        int? zeroCol = xMin <= 0 && 0 <= xMax ? MapXToColumn(0, xMin, xMax, width) : null;
        int? zeroRow = yMin <= 0 && 0 <= yMax ? MapYToRow(0, yMin, yMax, height) : null;

        if (zeroCol is { } col)
            for (var row = 0; row < height; row++)
                cells[row, col] = CellKind.AxisVertical;

        if (zeroRow is { } row2)
            for (var c = 0; c < width; c++)
                cells[row2, c] = cells[row2, c] == CellKind.AxisVertical ? CellKind.Origin : CellKind.AxisHorizontal;
    }

    private static void DrawCurve(
        CellKind[,] cells, IReadOnlyList<PlotPoint> points, double xMin, double xMax, double yMin, double yMax,
        int width, int height)
    {
        if (points.Count == 0) return;

        for (var col = 0; col < width; col++)
        {
            var x = xMin + (xMax - xMin) * col / Math.Max(1, width - 1);
            var point = points[NearestIndex(points, x)];

            if (point.Y is not { } y) continue;

            var row = MapYToRow(y, yMin, yMax, height);
            cells[row, col] = CellKind.Curve;
        }
    }

    private static void DrawExtrema(
        CellKind[,] cells, IReadOnlyList<Extremum> extrema, double xMin, double xMax, double yMin, double yMax,
        int width, int height)
    {
        foreach (var extremum in extrema)
        {
            if (extremum.X < xMin || extremum.X > xMax) continue;

            var col = MapXToColumn(extremum.X, xMin, xMax, width);
            var row = MapYToRow(extremum.Y, yMin, yMax, height);
            cells[row, col] = extremum.Kind == ExtremumKind.Maximum ? CellKind.Maximum : CellKind.Minimum;
        }
    }

    private static int NearestIndex(IReadOnlyList<PlotPoint> points, double x)
    {
        var lo = 0;
        var hi = points.Count - 1;

        if (x <= points[0].X) return 0;
        if (x >= points[hi].X) return hi;

        while (lo < hi)
        {
            var mid = (lo + hi) / 2;
            if (points[mid].X < x) lo = mid + 1;
            else hi = mid;
        }

        if (lo == 0) return 0;
        var before = points[lo - 1];
        var after = points[lo];
        return (x - before.X) <= (after.X - x) ? lo - 1 : lo;
    }

    private static int MapXToColumn(double x, double xMin, double xMax, int width)
    {
        var t = xMax > xMin ? (x - xMin) / (xMax - xMin) : 0;
        return Math.Clamp((int)Math.Round(t * (width - 1)), 0, width - 1);
    }

    private static int MapYToRow(double y, double yMin, double yMax, int height)
    {
        var t = yMax > yMin ? (y - yMin) / (yMax - yMin) : 0;
        var row = (int)Math.Round((1 - t) * (height - 1));
        return Math.Clamp(row, 0, height - 1);
    }

    private static string RenderCell(CellKind kind) => kind switch
    {
        CellKind.Curve => "[cyan]*[/]",
        CellKind.Maximum => "[green]^[/]",
        CellKind.Minimum => "[red]v[/]",
        CellKind.Origin => "[grey]+[/]",
        CellKind.AxisVertical => "[grey]|[/]",
        CellKind.AxisHorizontal => "[grey]-[/]",
        _ => " ",
    };

    private static string FormatNumber(double value) => value.ToString("0.####", CultureInfo.InvariantCulture);

    private static int DetectConsoleWidth()
    {
        try
        {
            var width = AnsiConsole.Profile.Width;
            return width > 0 ? width : FallbackWidth;
        }
        catch
        {
            return FallbackWidth;
        }
    }

    private static int DetectConsoleHeight()
    {
        try
        {
            var height = Console.WindowHeight;
            return height > 0 ? height : FallbackHeight;
        }
        catch
        {
            return FallbackHeight;
        }
    }
}
