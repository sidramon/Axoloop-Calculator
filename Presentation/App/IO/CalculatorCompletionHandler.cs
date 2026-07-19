namespace Presentation.App.IO;

using ReadLineReboot;
using Application.Calculator;

public sealed class CalculatorCompletionHandler : IAutoCompleteHandler
{
    private readonly IReadOnlyList<string> _functionNames;
    private readonly IReadOnlyList<string> _metaCommands;
    private readonly ListVariablesUseCase _listVariables;
    private readonly ListFunctionsUseCase _listFunctions;

    public char[] Separators { get; set; } = { ' ', '(', ',', '+', '-', '*', '/', '^', '[', ';' };

    public CalculatorCompletionHandler(
        IReadOnlyList<string> functionNames,
        IReadOnlyList<string> metaCommands,
        ListVariablesUseCase listVariables,
        ListFunctionsUseCase listFunctions)
    {
        _functionNames = functionNames;
        _metaCommands = metaCommands;
        _listVariables = listVariables;
        _listFunctions = listFunctions;
    }

    public string[] GetSuggestions(string text, int index)
    {
        if (string.IsNullOrEmpty(text) || index > text.Length)
            return Array.Empty<string>();

        var prefix = text[index..];

        if (prefix.StartsWith('/'))
            return _metaCommands
                .Where(c => c.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .OrderBy(c => c, StringComparer.Ordinal)
                .ToArray();

        if (prefix.Length == 0)
            return Array.Empty<string>();

        var variables = _listVariables.Execute().Select(v => v.Name);
        var userFunctions = _listFunctions.Execute().Select(f => f.Name);

        return _functionNames
            .Concat(variables)
            .Concat(userFunctions)
            .Where(n => n.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .Distinct()
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToArray();
    }
}