namespace Presentation.App;

using System.Globalization;
using Domain.Calculator.Symbolic;
using Domain.Calculator.Values;

public sealed class NumberFormatter
{
    private readonly FormatOptions _options;
    private readonly string _pattern;

    public NumberFormatter(FormatOptions options)
    {
        _options = options;
        _pattern = "0." + new string('#', options.Precision);
    }

    public string Format(Value value) => value switch
    {
        NumberValue n  => FormatNumber(n.Number),
        BooleanValue b => b.Boolean ? "True" : "False",
        FunctionDefinedValue f => $"{f.Name}({string.Join(", ", f.Parameters)}) defined",
        FunctionValue fn => fn.Signature,
        SolutionValue s => FormatSolutionInline(s),
        SymbolicValue s => SymbolicPrinter.Print(s.Expression),
        _ => value.ToString() ?? ""
    };

    private string FormatSolutionInline(SolutionValue solution)
    {
        var roots = string.Join(", ", solution.Values.Select(v => $"{solution.Unknown} = {FormatNumber(v)}"));
        var omitted = solution.TotalFound - solution.Values.Count;
        return omitted > 0 ? $"{roots} (+{omitted} more)" : roots;
    }

    /// <summary>One "unknown = value" line per root, for a multi-line top-level echo.</summary>
    public IReadOnlyList<string> FormatSolutionLines(SolutionValue solution) =>
        solution.Values.Select(v => $"{solution.Unknown} = {FormatNumber(v)}").ToList();

    /// <summary>
    /// Non-null only when roots were capped for display — names the real count and points
    /// at the explicit-domain overload to narrow it down.
    /// </summary>
    public string? FormatSolutionHint(SolutionValue solution) =>
        solution.TotalFound > solution.Values.Count
            ? $"{solution.TotalFound} roots found in total; showing the first {solution.Values.Count}. " +
              "Restrict the domain with solve(equation, unknown, xMin, xMax) to narrow it down."
            : null;

    public string FormatNumber(double x)
    {
        if (double.IsNaN(x))      return "NaN";
        if (double.IsInfinity(x)) return x > 0 ? "Infinity" : "-Infinity";

        if (Math.Abs(x) < _options.MinValue) x = 0;

        if (x != 0 && Math.Abs(x) >= _options.MaxValue)
            return x.ToString("0.####E+0", CultureInfo.InvariantCulture);

        var rounded = Math.Round(x, _options.Precision);
        return rounded.ToString(_pattern, CultureInfo.InvariantCulture);
    }
}