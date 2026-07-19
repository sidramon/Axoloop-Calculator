namespace Domain.Calculator;

using Domain.Calculator.Ast;

public sealed record UserFunction(string Name, IReadOnlyList<string> Parameters, IExpression Body);
