namespace Application.Calculator.Documentation;

using Domain.Calculator.Operations.Functions;

public sealed record FunctionCategoryGroup(FunctionCategory Category, IReadOnlyList<FunctionDoc> Functions);
