namespace Domain.Calculator.Ast;

using Domain.Calculator.Operations;

public sealed record BinaryExpression(IExpression Left, IOperator Operator, IExpression Right) : IExpression;