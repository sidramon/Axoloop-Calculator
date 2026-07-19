namespace Domain.Calculator.Ast;

public sealed record IdentifierExpression(string Name) : IExpression;