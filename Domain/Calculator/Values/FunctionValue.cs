namespace Domain.Calculator.Values;

public sealed record FunctionValue : Value
{
    public string Name { get; }
    public int Arity { get; }
    public string Signature { get; }

    private readonly Func<IReadOnlyList<Value>, Value> _invoke;

    public FunctionValue(string name, int arity, string signature, Func<IReadOnlyList<Value>, Value> invoke)
    {
        Name = name;
        Arity = arity;
        Signature = signature;
        _invoke = invoke;
    }

    public Value Invoke(IReadOnlyList<Value> arguments)
    {
        if (arguments.Count != Arity)
            throw new InvalidOperationException($"'{Name}' expects {Arity} argument(s) but got {arguments.Count}.");
        return _invoke(arguments);
    }
}
