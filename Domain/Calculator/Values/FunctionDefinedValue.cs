namespace Domain.Calculator.Values;

public sealed record FunctionDefinedValue(string Name, IReadOnlyList<string> Parameters) : Value;
