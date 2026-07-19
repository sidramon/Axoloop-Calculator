namespace Application.Calculator;

using Domain.Calculator;
using Domain.Calculator.Values;

public sealed record VariableEntry(string Name, Value Value, bool IsConstant);

public sealed class ListVariablesUseCase
{
    private readonly VariableContext _context;

    public ListVariablesUseCase(VariableContext context) => _context = context;

    public IReadOnlyList<VariableEntry> Execute() =>
        _context.All
            .OrderBy(kv => _context.IsProtected(kv.Key))
            .ThenBy(kv => kv.Key, StringComparer.Ordinal)
            .Select(kv => new VariableEntry(kv.Key, kv.Value, _context.IsProtected(kv.Key)))
            .ToList();
}