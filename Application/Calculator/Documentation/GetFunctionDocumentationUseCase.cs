namespace Application.Calculator.Documentation;

using Domain.Calculator.Operations.Functions;
using Domain.Calculator.Operations.SpecialForms;

public sealed class GetFunctionDocumentationUseCase
{
    private readonly IReadOnlyList<FunctionDoc> _all;

    public GetFunctionDocumentationUseCase(IEnumerable<IFunction> functions, IEnumerable<ISpecialForm> specialForms)
    {
        // A special form may be registered more than once under the same name to support
        // multiple arities (e.g. solve/2 and solve/4) — collapse those back into a single
        // documentation entry rather than listing the same name twice.
        _all = functions.Select(FunctionDoc.From)
            .Concat(specialForms.Select(FunctionDoc.From))
            .GroupBy(f => f.Name, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToList();
    }

    public IReadOnlyList<FunctionCategoryGroup> Execute() =>
        _all
            .GroupBy(f => f.Category)
            .OrderBy(g => g.Key)
            .Select(g => new FunctionCategoryGroup(
                g.Key,
                g.OrderBy(f => f.Name, StringComparer.Ordinal).ToList()))
            .ToList();

    public FunctionDoc? Execute(string name) =>
        _all.FirstOrDefault(f => string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase));

    public IReadOnlyList<string> SuggestSimilarNames(string name, int maxSuggestions = 3) =>
        _all
            .Select(f => f.Name)
            .Select(candidate => (Name: candidate, SharedPrefixLength: CommonPrefixLength(name, candidate)))
            .Where(x => x.SharedPrefixLength > 0)
            .OrderByDescending(x => x.SharedPrefixLength)
            .ThenBy(x => x.Name, StringComparer.Ordinal)
            .Take(maxSuggestions)
            .Select(x => x.Name)
            .ToList();

    private static int CommonPrefixLength(string a, string b)
    {
        var length = Math.Min(a.Length, b.Length);
        var i = 0;
        while (i < length && char.ToLowerInvariant(a[i]) == char.ToLowerInvariant(b[i])) i++;
        return i;
    }
}
