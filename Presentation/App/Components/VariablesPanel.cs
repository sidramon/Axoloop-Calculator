namespace Presentation.App.Components;

using Spectre.Console;
using Application.Calculator;
using Domain.Calculator.Values;

public static class VariablesPanel
{
    public static void Render(IReadOnlyList<VariableEntry> entries, NumberFormatter formatter)
    {
        if (entries.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey]No variables defined.[/]");
            return;
        }

        var table = new Table()
            .Border(TableBorder.None)
            .HideHeaders()
            .AddColumn(new TableColumn("").PadRight(4))
            .AddColumn("");

        foreach (var entry in entries)
        {
            var color = entry.IsConstant ? "grey" : "cyan";
            var display = entry.Value is MatrixValue m
                ? $"{m.Rows}x{m.Columns} matrix"
                : formatter.Format(entry.Value);

            table.AddRow($"[{color}]{Markup.Escape(entry.Name)}[/]", Markup.Escape(display));
        }

        var panel = new Panel(table)
        {
            Header = new PanelHeader("[bold cyan] Variables [/]").Centered(),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(foreground: Color.Cyan1),
            Padding = new Padding(2, 1),
        };

        AnsiConsole.Write(panel);
    }
}