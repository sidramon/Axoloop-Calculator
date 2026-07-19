namespace Presentation.App.Components;

using Spectre.Console;
using Application.Calculator;

public static class FunctionsPanel
{
    public static void Render(IReadOnlyList<UserFunctionEntry> entries)
    {
        if (entries.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey]No functions defined.[/]");
            return;
        }

        var table = new Table()
            .Border(TableBorder.None)
            .HideHeaders()
            .AddColumn("");

        foreach (var entry in entries)
        {
            var signature = $"{entry.Name}({string.Join(", ", entry.Parameters)})";
            table.AddRow($"[cyan]{Markup.Escape(signature)}[/]");
        }

        var panel = new Panel(table)
        {
            Header = new PanelHeader("[bold cyan] Functions [/]").Centered(),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(foreground: Color.Cyan1),
            Padding = new Padding(2, 1),
        };

        AnsiConsole.Write(panel);
    }
}
