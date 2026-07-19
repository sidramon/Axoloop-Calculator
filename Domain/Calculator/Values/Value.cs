namespace Domain.Calculator.Values;

public abstract record Value;

public sealed record NumberValue(double Number) : Value;

public sealed record BooleanValue(bool Boolean) : Value;