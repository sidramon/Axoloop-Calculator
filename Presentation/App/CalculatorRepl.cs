namespace Presentation.App;

using ReadLineReboot;
using Spectre.Console;
using Presentation.App.Components;

public sealed class CalculatorRepl
{
    private readonly CommandRouter _router;
    private readonly IAutoCompleteHandler _completionHandler;

    public CalculatorRepl(CommandRouter router, IAutoCompleteHandler completionHandler)
    {
        _router = router;
        _completionHandler = completionHandler;
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        ReadLine.HistoryEnabled = true;
        ReadLine.AutoCompletionEnabled = true;
        ReadLine.AutoCompletionHandler = _completionHandler;

        WelcomeBanner.Render();

        while (!ct.IsCancellationRequested)
        {
            var input = ReadLine.Read("Axoloop> ");
            if (input is null) break;

            input = input.Trim();
            if (input.Length == 0) continue;
            if (input is "exit" or "quit") break;

            await _router.HandleAsync(input, ct);
        }

        AnsiConsole.MarkupLine("[cyan]Closing.[/]");
    }
}