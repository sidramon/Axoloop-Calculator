using Spectre.Console.Rendering;

namespace Presentation.App.Components;

using Spectre.Console;
using Domain.Calculator.Values;

/// <summary>
/// The table ready to write, plus an optional note to print alongside it. Kept separate
/// from Spectre's own <c>Table.Title</c>: a title wider than the table body gets wrapped
/// and truncated to the body's width, which is unreadable for a single narrow column.
/// </summary>
public sealed record MatrixRenderResult(IRenderable Table, string? Note);

public static class MatrixRenderer
{
    private const int DefaultConsoleWidth = 80;

    // A right-aligned numeric cell plus its padding and border column costs roughly this
    // many characters; used only to estimate how many columns can fit, not to lay out
    // anything precisely.
    private const int EstimatedColumnWidth = 8;
    private const int MinColumnsBeforeTransposing = 4;

    /// <summary>
    /// Renders a matrix as a table. A 1xN row vector wider than the console can
    /// comfortably show — Spectre otherwise collapses a table with far more columns than
    /// fit into an unreadable "…" — is instead rendered as a column, with a note on the
    /// real shape so the transposition isn't mistaken for the actual data.
    /// </summary>
    public static MatrixRenderResult Render(
        MatrixValue matrix, Func<double, string> formatCell, int consoleWidth = DefaultConsoleWidth)
    {
        var maxColumns = Math.Max(MinColumnsBeforeTransposing, consoleWidth / EstimatedColumnWidth);

        if (matrix.Rows == 1 && matrix.Columns > maxColumns)
        {
            var note = $"{matrix.Rows}x{matrix.Columns} (shown as column)";
            return new MatrixRenderResult(RenderColumn(matrix, formatCell), note);
        }

        return new MatrixRenderResult(RenderNormal(matrix, formatCell), null);
    }

    private static Table RenderNormal(MatrixValue matrix, Func<double, string> formatCell)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Cyan1)
            .HideHeaders();

        for (var c = 0; c < matrix.Columns; c++)
            table.AddColumn(new TableColumn("").RightAligned());

        for (var r = 0; r < matrix.Rows; r++)
        {
            var cells = new string[matrix.Columns];
            for (var c = 0; c < matrix.Columns; c++)
                cells[c] = formatCell(matrix[r, c]);
            table.AddRow(cells);
        }

        return table;
    }

    private static Table RenderColumn(MatrixValue matrix, Func<double, string> formatCell)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Cyan1)
            .HideHeaders()
            .AddColumn(new TableColumn("").RightAligned());

        for (var c = 0; c < matrix.Columns; c++)
            table.AddRow(formatCell(matrix[0, c]));

        return table;
    }
}
