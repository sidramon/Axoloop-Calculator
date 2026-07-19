namespace Domain.Calculator.Ast;

using Domain.Calculator.Operations;

public sealed record UnaryExpression(IExpression Operand, IUnaryOperator Operator) : IExpression;