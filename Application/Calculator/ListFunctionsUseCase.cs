namespace Application.Calculator;

using Domain.Calculator;

public sealed record UserFunctionEntry(string Name, IReadOnlyList<string> Parameters);

public sealed class ListFunctionsUseCase
{
    private readonly FunctionContext _context;

    public ListFunctionsUseCase(FunctionContext context) => _context = context;

    public IReadOnlyList<UserFunctionEntry> Execute() =>
        _context.All
            .OrderBy(kv => kv.Key, StringComparer.Ordinal)
            .Select(kv => new UserFunctionEntry(kv.Value.Name, kv.Value.Parameters))
            .ToList();
}
