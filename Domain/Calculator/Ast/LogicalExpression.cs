namespace Domain.Calculator.Ast;

public sealed record LogicalExpression(IExpression Left, LogicalOperator Operator, IExpression Right) : IExpression;
