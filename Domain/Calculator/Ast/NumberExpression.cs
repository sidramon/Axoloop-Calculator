namespace Domain.Calculator.Ast;

using Domain.Calculator.Values;

public sealed record NumberExpression(Value Value) : IExpression;