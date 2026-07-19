using Spectre.Console.Rendering;

namespace Presentation.App.Components;

using Spectre.Console;
using Domain.Calculator.Values;

public static class MatrixRenderer
{
    public static IRenderable Render(MatrixValue matrix, Func<double, string> formatCell)
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
}