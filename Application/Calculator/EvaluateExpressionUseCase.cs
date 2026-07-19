namespace Application.Calculator;

using Domain.Calculator;
using Domain.Calculator.Parsing;
using Domain.Calculator.Values;

public sealed class EvaluateExpressionUseCase
{
    private readonly Parser _parser;
    private readonly Evaluator _evaluator;
    private readonly VariableContext _context;

    public EvaluateExpressionUseCase(Parser parser, Evaluator evaluator, VariableContext context)
    {
        _parser = parser;
        _evaluator = evaluator;
        _context = context;
    }

    public Task<Value> ExecuteAsync(string input, CancellationToken ct)
    {
        var ast = _parser.Parse(input);
        var result = _evaluator.Evaluate(ast, _context);
        return Task.FromResult(result);
    }
}