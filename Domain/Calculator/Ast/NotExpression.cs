namespace Domain.Calculator.Ast;

public sealed record NotExpression(IExpression Operand) : IExpression;
