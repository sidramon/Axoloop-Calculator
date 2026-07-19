namespace Domain.Calculator.Ast;

public sealed record MatrixExpression(IReadOnlyList<IReadOnlyList<IExpression>> Rows) : IExpression;