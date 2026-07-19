namespace Domain.Calculator;

using Domain.Calculator.Values;

public sealed class VariableContext
{
    private readonly VariableContext? _parent;
    private readonly Dictionary<string, Value> _variables = new();
    private readonly HashSet<string> _protected = new();

    public VariableContext() : this(null) { }

    private VariableContext(VariableContext? parent) => _parent = parent;

    public VariableContext CreateChild() => new(this);

    public IReadOnlyDictionary<string, Value> All => _variables;

    public bool IsProtected(string name) =>
        _protected.Contains(name) || (_parent?.IsProtected(name) ?? false);

    public void Seed(IReadOnlyDictionary<string, Value> constants)
    {
        foreach (var (name, value) in constants)
        {
            _variables[name] = value;
            _protected.Add(name);
        }
    }

    public void Set(string name, Value value)
    {
        if (IsProtected(name))
            throw new InvalidOperationException($"'{name}' is a constant and cannot be reassigned.");
        _variables[name] = value;
    }

    public void Bind(string name, Value value) => _variables[name] = value;

    public Value Get(string name) =>
        _variables.TryGetValue(name, out var value)
            ? value
            : _parent is not null
                ? _parent.Get(name)
                : throw new InvalidOperationException($"Undefined variable '{name}'.");

    public bool IsDefined(string name) => _variables.ContainsKey(name) || (_parent?.IsDefined(name) ?? false);
}