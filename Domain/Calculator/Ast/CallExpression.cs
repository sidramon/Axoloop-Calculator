namespace Domain.Calculator.Ast;

public sealed record CallExpression(string Name, IReadOnlyList<IExpression> Arguments) : IExpression;
