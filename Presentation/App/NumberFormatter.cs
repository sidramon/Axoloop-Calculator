namespace Presentation.App;

using System.Globalization;
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
        _ => value.ToString() ?? ""
    };

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