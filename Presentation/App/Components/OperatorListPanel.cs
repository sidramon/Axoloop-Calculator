namespace Presentation.App.Components;

using Spectre.Console;
using Application.Calculator.Documentation;

public static class OperatorListPanel
{
    public static void Render(IReadOnlyList<OperatorDoc> operators)
    {
        var table = new Table()
            .Border(TableBorder.Simple)
            .AddColumn("Symbol")
            .AddColumn("Kind")
            .AddColumn("Prec.")
            .AddColumn("Assoc.")
            .AddColumn("Description");

        foreach (var op in operators)
        {
            table.AddRow(
                $"[cyan]{Markup.Escape(op.Symbol)}[/]",
                Markup.Escape(op.Kind.ToString()),
                op.Precedence?.ToString() ?? "—",
                op.Associativity?.ToString() ?? "—",
                Markup.Escape(op.Description));
        }

        var panel = new Panel(table)
        {
            Header = new PanelHeader("[bold cyan] Operators [/]").Centered(),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(foreground: Color.Cyan1),
            Padding = new Padding(1, 0),
        };

        AnsiConsole.Write(panel);
    }
}
