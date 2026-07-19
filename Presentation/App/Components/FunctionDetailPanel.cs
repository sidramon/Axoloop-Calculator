namespace Presentation.App.Components;

using Spectre.Console;
using Application.Calculator.Documentation;

public static class FunctionDetailPanel
{
    public static void Render(FunctionDoc function)
    {
        var table = new Table()
            .Border(TableBorder.None)
            .HideHeaders()
            .AddColumn(new TableColumn("").PadRight(4))
            .AddColumn("");

        table.AddRow("[cyan]Signature[/]", Markup.Escape(function.Signature));
        table.AddRow("[cyan]Category[/]", Markup.Escape(function.Category.ToString()));
        table.AddRow("[cyan]Description[/]", Markup.Escape(function.Description));

        if (function.Examples.Count > 0)
        {
            table.AddEmptyRow();
            table.AddRow("[cyan]Examples[/]", "");
            foreach (var example in function.Examples)
                table.AddRow("", Markup.Escape(example));
        }

        var panel = new Panel(table)
        {
            Header = new PanelHeader($"[bold cyan] {Markup.Escape(function.Name)} [/]").Centered(),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(foreground: Color.Cyan1),
            Padding = new Padding(2, 1),
        };

        AnsiConsole.Write(panel);
    }
}
