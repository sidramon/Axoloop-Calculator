namespace Presentation.App.Components;

using Spectre.Console;
using Application.Calculator.Documentation;

public static class FunctionListPanel
{
    public static void Render(IReadOnlyList<FunctionCategoryGroup> groups)
    {
        var table = new Table()
            .Border(TableBorder.None)
            .HideHeaders()
            .AddColumn(new TableColumn("").PadRight(4))
            .AddColumn("");

        var first = true;
        foreach (var group in groups)
        {
            if (!first) table.AddEmptyRow();
            first = false;

            table.AddRow($"[bold cyan]{Markup.Escape(group.Category.ToString())}[/]", "");
            foreach (var function in group.Functions)
                table.AddRow($"  [cyan]{Markup.Escape(function.Name)}[/]", Markup.Escape(function.Signature));
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
