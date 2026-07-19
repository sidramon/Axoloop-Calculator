namespace Domain.Calculator.Ast;

public sealed record FunctionDefinitionExpression(string Name, IReadOnlyList<string> Parameters, IExpression Body) : IExpression;
