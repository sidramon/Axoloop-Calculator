namespace Presentation.App;

using System.Globalization;
using Domain.Calculator.Algorithms;
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
        ComplexValue c => FormatComplex(c),
        BooleanValue b => b.Boolean ? "True" : "False",
        FunctionDefinedValue f => $"{f.Name}({string.Join(", ", f.Parameters)}) defined",
        FunctionValue fn => fn.Signature,
        SolutionValue s => FormatSolutionInline(s),
        SymbolicValue s => SymbolicPrinter.Print(s.Expression),
        ValueListValue l => $"[{string.Join(", ", l.Values.Select(Format))}]",
        LimitValue lim => FormatLimit(lim),
        _ => value.ToString() ?? ""
    };

    /// <summary>
    /// A one-sided lim(..., direction) result (Direction != null) shows only the
    /// requested side. A two-sided result shows both sides only when they genuinely
    /// disagree (OneSidedDiffer) — every other outcome (converge, diverge, no limit) is
    /// one value for both sides, so it's shown once.
    /// </summary>
    private string FormatLimit(LimitValue limit)
    {
        var targetText = double.IsPositiveInfinity(limit.Target) ? "∞"
            : double.IsNegativeInfinity(limit.Target) ? "-∞"
            : FormatNumber(limit.Target);

        if (limit.Direction is { } direction)
        {
            var arrow = $"{limit.Variable} → {targetText}{(direction < 0 ? "⁻" : "⁺")}";
            return $"{arrow} : {FormatLimitOutcome(limit.Kind, limit.Value)}";
        }

        if (limit.Kind == LimitKind.OneSidedDiffer)
            return $"{limit.Variable} → {targetText}⁻ : {FormatSignedInfinityOrNumber(limit.LeftValue)}, " +
                   $"{limit.Variable} → {targetText}⁺ : {FormatSignedInfinityOrNumber(limit.RightValue)} " +
                   "(two-sided limit does not exist)";

        return $"{limit.Variable} → {targetText} : {FormatLimitOutcome(limit.Kind, limit.Value)}";
    }

    private string FormatLimitOutcome(LimitKind kind, double? value) => kind switch
    {
        LimitKind.Converges => FormatNumber(value!.Value),
        LimitKind.DivergesToPositiveInfinity => "+∞",
        LimitKind.DivergesToNegativeInfinity => "-∞",
        _ => "no limit (oscillates)"
    };

    private string FormatSignedInfinityOrNumber(double? value)
    {
        if (value is null) return "no limit";
        if (double.IsPositiveInfinity(value.Value)) return "+∞";
        if (double.IsNegativeInfinity(value.Value)) return "-∞";
        return FormatNumber(value.Value);
    }

    /// <summary>
    /// Each component is rendered through <see cref="FormatNumber"/> first, so snap-to-zero
    /// and precision stay consistent with plain real output. A magnitude that formats as "1"
    /// is shown as a bare "i" (<c>1i → i</c>, <c>-1i → -i</c>); a real part that formats as
    /// "0" is omitted entirely. Construction always reduces a negligible imaginary part to a
    /// real <see cref="NumberValue"/>, so that branch here is a defensive fallback rather
    /// than a reachable case.
    /// </summary>
    private string FormatComplex(ComplexValue c)
    {
        var zeroText = FormatNumber(0);
        var realText = FormatNumber(c.Real);
        var magnitudeText = FormatNumber(Math.Abs(c.Imaginary));

        if (magnitudeText == zeroText) return realText;

        var imaginaryPart = magnitudeText == FormatNumber(1) ? "i" : $"{magnitudeText}i";
        var sign = c.Imaginary < 0 ? "-" : "+";

        if (realText == zeroText)
            return c.Imaginary < 0 ? $"-{imaginaryPart}" : imaginaryPart;

        return $"{realText} {sign} {imaginaryPart}";
    }

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