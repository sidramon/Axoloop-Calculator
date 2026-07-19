namespace Presentation.App.Components;

using Spectre.Console;

public static class WelcomeBanner
{
    public static void Render()
    {
        var content = new Markup("""
                                 [grey]/help[/]   show available commands
                                 [grey]exit[/]    quit the calculator
                                 """);

        var panel = new Panel(content)
        {
            Header      = new PanelHeader("[bold cyan] Axoloop Calculator [/]").Centered(),
            Border      = BoxBorder.Rounded,
            BorderStyle = new Style(foreground: Color.Cyan1),
            Padding     = new Padding(2, 1),
        };

        AnsiConsole.Write(panel);
    }
}