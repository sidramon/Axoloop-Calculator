namespace Application.Calculator.Documentation;

using Domain.Calculator.Operations.Functions;
using Domain.Calculator.Operations.SpecialForms;

public sealed record FunctionDoc(
    string Name,
    FunctionCategory Category,
    string Signature,
    int Arity,
    string Description,
    IReadOnlyList<string> Examples)
{
    public static FunctionDoc From(IFunction function) => new(
        function.Name,
        function.Category,
        function.Signature,
        function.Arity,
        function.Description,
        function.Examples);

    public static FunctionDoc From(ISpecialForm specialForm) => new(
        specialForm.Name,
        specialForm.Category,
        specialForm.Signature,
        specialForm.Arity,
        specialForm.Description,
        specialForm.Examples);
}
