namespace Presentation.Tests.App.Components;

using Presentation.App.Components;
using Domain.Calculator.Values;
using Spectre.Console;
using FluentAssertions;

public class MatrixRendererTests
{
    private static string Format(double x) => x.ToString("0.##");

    [Fact]
    public void Render_NormalSquareMatrix_RendersUnchangedWithoutTransposing()
    {
        var matrix = new MatrixValue(new double[,] { { 1, 2 }, { 3, 4 } });

        var result = MatrixRenderer.Render(matrix, Format, consoleWidth: 80);
        var table = (Table)result.Table;

        table.Columns.Count.Should().Be(2);
        table.Rows.Count.Should().Be(2);
        result.Note.Should().BeNull();
    }

    [Fact]
    public void Render_WideRowVectorExceedingConsoleWidth_TransposesToColumnAndNotesRealShape()
    {
        var data = new double[1, 40];
        for (var i = 0; i < 40; i++) data[0, i] = i;
        var matrix = new MatrixValue(data);

        var result = MatrixRenderer.Render(matrix, Format, consoleWidth: 80);
        var table = (Table)result.Table;

        table.Columns.Count.Should().Be(1);
        table.Rows.Count.Should().Be(40);
        result.Note.Should().NotBeNull();
        result.Note.Should().Contain("1x40");
    }

    [Fact]
    public void Render_WideColumnVector_IsNotTransposed_AlreadyReadableAsIs()
    {
        var data = new double[40, 1];
        for (var i = 0; i < 40; i++) data[i, 0] = i;
        var matrix = new MatrixValue(data);

        var result = MatrixRenderer.Render(matrix, Format, consoleWidth: 80);
        var table = (Table)result.Table;

        table.Columns.Count.Should().Be(1);
        table.Rows.Count.Should().Be(40);
        result.Note.Should().BeNull();
    }

    [Fact]
    public void Render_RowVectorNarrowerThanThreshold_IsNotTransposed()
    {
        var matrix = new MatrixValue(new double[,] { { 1, 2, 3 } });

        var result = MatrixRenderer.Render(matrix, Format, consoleWidth: 80);
        var table = (Table)result.Table;

        table.Columns.Count.Should().Be(3);
        table.Rows.Count.Should().Be(1);
        result.Note.Should().BeNull();
    }

    [Fact]
    public void Render_NarrowerConsole_LowersTheColumnThreshold()
    {
        var matrix = new MatrixValue(new double[,] { { 1, 2, 3, 4, 5, 6 } });

        var result = MatrixRenderer.Render(matrix, Format, consoleWidth: 20);
        var table = (Table)result.Table;

        table.Columns.Count.Should().Be(1);
        table.Rows.Count.Should().Be(6);
        result.Note.Should().Contain("1x6");
    }
}
