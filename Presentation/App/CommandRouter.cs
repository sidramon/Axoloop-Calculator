namespace Presentation.App;

using System.Globalization;
using Spectre.Console;
using Presentation.App.Components;
using Application.Calculator;
using Application.Calculator.Documentation;
using Application.Calculator.Plotting;
using Application.Views;
using Domain.Calculator.Algorithms;
using Domain.Calculator.Values;

public sealed class CommandRouter
{
    private const double DefaultPlotXMin = -10;
    private const double DefaultPlotXMax = 10;
    private const int AsciiSampleCount = 400;
    private const int WebSampleCount = 2000;

    private readonly EvaluateExpressionUseCase _evaluate;
    private readonly ListVariablesUseCase _listVariables;
    private readonly ListFunctionsUseCase _listFunctions;
    private readonly GetFunctionDocumentationUseCase _functionDocs;
    private readonly GetOperatorDocumentationUseCase _operatorDocs;
    private readonly IDocumentationRenderer _documentationRenderer;
    private readonly PlotFunctionUseCase _plotFunction;
    private readonly IReadOnlyDictionary<PlotFormat, IPlotRenderer> _plotRenderers;
    private readonly IViewLauncher _viewLauncher;
    private readonly NumberFormatter _formatter;

    public CommandRouter(
        EvaluateExpressionUseCase evaluate,
        ListVariablesUseCase listVariables,
        ListFunctionsUseCase listFunctions,
        GetFunctionDocumentationUseCase functionDocs,
        GetOperatorDocumentationUseCase operatorDocs,
        IDocumentationRenderer documentationRenderer,
        PlotFunctionUseCase plotFunction,
        IEnumerable<IPlotRenderer> plotRenderers,
        IViewLauncher viewLauncher,
        NumberFormatter formatter)
    {
        _evaluate = evaluate;
        _listVariables = listVariables;
        _listFunctions = listFunctions;
        _functionDocs = functionDocs;
        _operatorDocs = operatorDocs;
        _documentationRenderer = documentationRenderer;
        _plotFunction = plotFunction;
        _plotRenderers = plotRenderers.ToDictionary(r => r.Format);
        _viewLauncher = viewLauncher;
        _formatter = formatter;
    }

    public async Task HandleAsync(string input, CancellationToken ct)
    {
        if (input.StartsWith('/'))
        {
            await HandleMetaCommand(input, ct);
            return;
        }

        await HandleExpressionAsync(input, ct);
    }

    private async Task HandleMetaCommand(string input, CancellationToken ct)
    {
        var parts = input.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var command = parts[0];
        var argument = parts.Length > 1 ? parts[1].Trim() : null;

        switch (command)
        {
            case "/help":
                if (argument is null) HelpPanel.Render();
                else HandleHelp(argument);
                break;

            case "/vars":
                VariablesPanel.Render(_listVariables.Execute(), _formatter);
                break;

            case "/funcs":
                FunctionsPanel.Render(_listFunctions.Execute());
                break;

            case "/functions":
                HandleFunctions(argument);
                break;

            case "/operators":
                OperatorListPanel.Render(_operatorDocs.Execute());
                break;

            case "/doc":
                OpenDocumentationPage();
                break;

            case "/plot":
                await HandlePlot(argument, ct);
                break;

            case "/plotweb":
                await HandlePlotWeb(argument, ct);
                break;

            case "/zeros":
                await HandleZeros(argument, ct);
                break;

            case "/extrema":
                await HandleExtrema(argument, ct);
                break;

            case "/clear":
                AnsiConsole.Clear();
                break;

            default:
                AnsiConsole.MarkupLine($"[red]Unknown command:[/] {Markup.Escape(input)}");
                break;
        }
    }

    private void HandleHelp(string name)
    {
        var doc = _functionDocs.Execute(name);
        if (doc is not null)
        {
            FunctionDetailPanel.Render(doc);
            return;
        }

        var suggestions = _functionDocs.SuggestSimilarNames(name);
        if (suggestions.Count > 0)
            AnsiConsole.MarkupLine(
                $"[red]Unknown function:[/] {Markup.Escape(name)}. " +
                $"[grey]Did you mean:[/] {Markup.Escape(string.Join(", ", suggestions))}?");
        else
            AnsiConsole.MarkupLine($"[red]Unknown function:[/] {Markup.Escape(name)}");
    }

    private void HandleFunctions(string? category)
    {
        var groups = _functionDocs.Execute();

        if (category is null)
        {
            FunctionListPanel.Render(groups);
            return;
        }

        var matched = groups.FirstOrDefault(
            g => string.Equals(g.Category.ToString(), category, StringComparison.OrdinalIgnoreCase));

        if (matched is null)
        {
            AnsiConsole.MarkupLine($"[red]Unknown category:[/] {Markup.Escape(category)}");
            return;
        }

        FunctionListPanel.Render(new[] { matched });
    }

    private void OpenDocumentationPage()
    {
        var functions = _functionDocs.Execute().SelectMany(g => g.Functions).ToList();
        var html = _documentationRenderer.Render(functions, _operatorDocs.Execute());
        _viewLauncher.Open(html, "html");
    }

    private async Task HandlePlot(string? argument, CancellationToken ct)
    {
        if (!TryParsePlotArguments(argument, out var name, out var xMin, out var xMax, out var error))
        {
            AnsiConsole.MarkupLine($"[red]{Markup.Escape(error!)}[/]");
            return;
        }

        try
        {
            var function = await ResolveFunctionValue(name, ct);
            var request = new PlotSampleRequest(SampleCount: AsciiSampleCount);
            var series = _plotFunction.Sample(function, xMin, xMax, request);
            var options = new PlotOptions(Width: 0, Height: 0, VisibleXMin: xMin, VisibleXMax: xMax);
            AnsiConsole.Markup(_plotRenderers[PlotFormat.Ascii].Render(series, options));
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");
        }
    }

    private async Task HandlePlotWeb(string? argument, CancellationToken ct)
    {
        if (!TryParsePlotArguments(argument, out var name, out var xMin, out var xMax, out var error))
        {
            AnsiConsole.MarkupLine($"[red]{Markup.Escape(error!)}[/]");
            return;
        }

        try
        {
            var function = await ResolveFunctionValue(name, ct);
            var request = new PlotSampleRequest(SampleCount: WebSampleCount, Oversample: true);
            var series = _plotFunction.Sample(function, xMin, xMax, request);
            var options = new PlotOptions(Width: 1000, Height: 600, VisibleXMin: xMin, VisibleXMax: xMax);
            var html = _plotRenderers[PlotFormat.Html].Render(series, options);
            _viewLauncher.Open(html, "html");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");
        }
    }

    private async Task HandleZeros(string? argument, CancellationToken ct)
    {
        if (!TryParsePlotArguments(argument, out var name, out var xMin, out var xMax, out var error))
        {
            AnsiConsole.MarkupLine($"[red]{Markup.Escape(error!)}[/]");
            return;
        }

        try
        {
            var function = await ResolveFunctionValue(name, ct);
            var request = new PlotSampleRequest(SampleCount: AsciiSampleCount);
            var series = _plotFunction.Sample(function, xMin, xMax, request);

            if (series.Zeros.Count == 0)
                AnsiConsole.MarkupLine("[grey]No zeros found in this domain.[/]");
            else
                AnsiConsole.MarkupLine(
                    $"[cyan]Zeros:[/] {string.Join(", ", series.Zeros.Select(FormatZero))}");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");
        }
    }

    private async Task HandleExtrema(string? argument, CancellationToken ct)
    {
        if (!TryParsePlotArguments(argument, out var name, out var xMin, out var xMax, out var error))
        {
            AnsiConsole.MarkupLine($"[red]{Markup.Escape(error!)}[/]");
            return;
        }

        try
        {
            var function = await ResolveFunctionValue(name, ct);
            var request = new PlotSampleRequest(SampleCount: AsciiSampleCount);
            var series = _plotFunction.Sample(function, xMin, xMax, request);

            if (series.Extrema.Count == 0)
            {
                AnsiConsole.MarkupLine("[grey]No local extrema found in this domain.[/]");
                return;
            }

            foreach (var extremum in series.Extrema.OrderBy(e => e.X))
            {
                var label = extremum.Kind == ExtremumKind.Maximum ? "[green]maximum[/]" : "[red]minimum[/]";
                AnsiConsole.MarkupLine(
                    $"{label} at x = {FormatZero(extremum.X)}, y = {FormatZero(extremum.Y)}");
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");
        }
    }

    private async Task<FunctionValue> ResolveFunctionValue(string name, CancellationToken ct)
    {
        var value = await _evaluate.ExecuteAsync(name, ct);
        if (value is not FunctionValue function)
            throw new InvalidOperationException($"'{name}' must evaluate to a function.");
        return function;
    }

    private static string FormatZero(double value) => value.ToString("0.####", CultureInfo.InvariantCulture);

    private static bool TryParsePlotArguments(
        string? argument, out string name, out double xMin, out double xMax, out string? error)
    {
        name = "";
        xMin = DefaultPlotXMin;
        xMax = DefaultPlotXMax;
        error = null;

        if (string.IsNullOrWhiteSpace(argument))
        {
            error = "Usage: <command> <expr> [xMin] [xMax]";
            return false;
        }

        var tokens = argument.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        name = tokens[0];

        if (tokens.Length == 1)
            return true;

        if (tokens.Length == 3
            && double.TryParse(tokens[1], NumberStyles.Float, CultureInfo.InvariantCulture, out xMin)
            && double.TryParse(tokens[2], NumberStyles.Float, CultureInfo.InvariantCulture, out xMax))
        {
            if (xMax > xMin)
                return true;

            error = $"Invalid domain: xMin ({tokens[1]}) must be less than xMax ({tokens[2]}).";
            return false;
        }

        error = "Usage: <command> <expr> [xMin] [xMax]";
        return false;
    }

    private async Task HandleExpressionAsync(string input, CancellationToken ct)
    {
        try
        {
            var result = await _evaluate.ExecuteAsync(input, ct);

            if (result is MatrixValue matrix)
            {
                var rendered = MatrixRenderer.Render(matrix, _formatter.FormatNumber, AnsiConsole.Profile.Width);
                if (rendered.Note is not null)
                    AnsiConsole.MarkupLine($"[grey]{Markup.Escape(rendered.Note)}[/]");
                AnsiConsole.Write(rendered.Table);
            }
            else if (result is SolutionValue solution)
            {
                foreach (var line in _formatter.FormatSolutionLines(solution))
                    AnsiConsole.MarkupLine($"[green]{Markup.Escape(line)}[/]");

                var hint = _formatter.FormatSolutionHint(solution);
                if (hint is not null)
                    AnsiConsole.MarkupLine($"[grey]{Markup.Escape(hint)}[/]");
            }
            else
            {
                AnsiConsole.MarkupLine($"[green]{Markup.Escape(_formatter.Format(result))}[/]");
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");
        }
    }
}
