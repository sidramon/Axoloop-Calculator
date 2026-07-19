namespace Presentation.App.Components;

using Spectre.Console;

public static class HelpPanel
{
    public static void Render()
    {
        var table = new Table()
            .Border(TableBorder.None)
            .HideHeaders()
            .AddColumn(new TableColumn("").PadRight(4))
            .AddColumn("");
        
        table.AddRow("[cyan]/help[/]",             "Show this help");
        table.AddRow("[cyan]/help <name>[/]",      "Show detailed help for one builtin function");
        table.AddRow("[cyan]/vars[/]",              "List defined variables");
        table.AddRow("[cyan]/funcs[/]",             "List user-defined functions");
        table.AddRow("[cyan]/functions[/]",         "List all builtin functions, grouped by category");
        table.AddRow("[cyan]/functions <category>[/]", "List builtin functions in one category");
        table.AddRow("[cyan]/operators[/]",         "List all operators with precedence and associativity");
        table.AddRow("[cyan]/doc[/]",               "Open the full documentation in a browser");
        table.AddRow("[cyan]/plot <fn>[/]",         "Plot a user-defined function as ASCII, domain [[-10, 10]]");
        table.AddRow("[cyan]/plot <fn> <lo> <hi>[/]", "Plot with an explicit domain");
        table.AddRow("[cyan]/plotweb <fn> [[lo hi]][/]", "Open an interactive plot in the browser");
        table.AddRow("[cyan]/zeros <fn> [[lo hi]][/]",  "List the zeros of a function without plotting");
        table.AddRow("[cyan]/extrema <fn> [[lo hi]][/]", "List local maxima/minima without plotting");
        table.AddRow("[cyan]/clear[/]",             "Clear the screen");
        table.AddRow("[cyan]exit[/]",               "Quit the calculator");

        table.AddEmptyRow();
        
        table.AddRow("[green]1 + 1[/]",          "Evaluate an expression");
        table.AddRow("[green]a := [[1,2;3,4]][/]", "Assign a matrix to a variable");
        table.AddRow("[green]a * 2[/]",           "Use a variable in an expression");

        var panel = new Panel(table)
        {
            Header      = new PanelHeader("[bold cyan] Help [/]").Centered(),
            Border      = BoxBorder.Rounded,
            BorderStyle = new Style(foreground: Color.Cyan1),
            Padding     = new Padding(2, 1),
        };

        AnsiConsole.Write(panel);
    }
}