namespace Application.Calculator.Documentation;

using Domain.Calculator.Operations;

public sealed record OperatorDoc(
    string Symbol,
    OperatorKind Kind,
    int? Precedence,
    Associativity? Associativity,
    string Description,
    string Example)
{
    public static OperatorDoc From(OperatorDocumentationEntry entry) => new(
        entry.Symbol,
        entry.Kind,
        entry.Precedence,
        entry.Associativity,
        entry.Description,
        entry.Example);
}
