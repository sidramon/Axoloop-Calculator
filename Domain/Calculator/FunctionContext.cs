namespace Domain.Calculator;

public sealed class FunctionContext
{
    private readonly Dictionary<string, UserFunction> _functions = new();

    public IReadOnlyDictionary<string, UserFunction> All => _functions;

    public void Define(UserFunction function) => _functions[function.Name] = function;

    public bool TryGet(string name, out UserFunction function) =>
        _functions.TryGetValue(name, out function!);
}
