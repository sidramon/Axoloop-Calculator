namespace Domain.Calculator;

using Domain.Calculator.Values;

public static class Constants
{
    public static IReadOnlyDictionary<string, Value> All { get; } = new Dictionary<string, Value>
    {
        ["_pi"] = new NumberValue(Math.PI),
        ["_e"]  = new NumberValue(Math.E),
        ["_tau"] = new NumberValue(Math.Tau),
        ["_phi"] = new NumberValue((1 + Math.Sqrt(5)) / 2),
        ["_inf"] = new NumberValue(double.PositiveInfinity),
        ["_nan"] = new NumberValue(double.NaN),
    };
}