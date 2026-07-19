namespace Domain.Calculator.Ast;

public sealed record AssignmentExpression(string Name, IExpression Value) : IExpression;