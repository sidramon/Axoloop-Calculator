namespace Application.Tests.Calculator;

using System.Threading;
using Application.Calculator;
using Domain.Calculator;
using Domain.Calculator.Operations;
using Domain.Calculator.Operations.Functions;
using Domain.Calculator.Operations.SpecialForms;
using Domain.Calculator.Parsing;
using Domain.Calculator.Values;
using FluentAssertions;

public class EvaluateExpressionUseCaseTests
{
    private static Parser CreateParser() => new(
        operators: new IOperator[] { new AddOperator(), new MultiplyOperator() },
        postfixOperators: Array.Empty<IUnaryOperator>(),
        prefixOperators: Array.Empty<IUnaryOperator>(),
        tokenizer: new Tokenizer());

    private static Evaluator CreateEvaluator() => new(
        builtins: Array.Empty<IFunction>(),
        specialForms: Array.Empty<ISpecialForm>(),
        functions: new FunctionContext());

    private static EvaluateExpressionUseCase CreateUseCase(VariableContext? context = null) =>
        new(CreateParser(), CreateEvaluator(), context ?? new VariableContext());

    [Fact]
    public async Task ExecuteAsync_ArithmeticExpression_ReturnsEvaluatedResult()
    {
        var useCase = CreateUseCase();

        var result = await useCase.ExecuteAsync("2+3*4", CancellationToken.None);

        result.Should().BeOfType<NumberValue>();
        ((NumberValue)result).Number.Should().BeApproximately(14, 1e-10);
    }

    [Fact]
    public async Task ExecuteAsync_AssignmentExpression_PersistsVariableInSharedContext()
    {
        var context = new VariableContext();
        var useCase = CreateUseCase(context);

        await useCase.ExecuteAsync("x := 2+3", CancellationToken.None);

        context.Get("x").Should().Be(new NumberValue(5));
    }

    [Fact]
    public async Task ExecuteAsync_InvalidSyntax_ThrowsFormatException()
    {
        var useCase = CreateUseCase();

        var act = async () => await useCase.ExecuteAsync("2+", CancellationToken.None);

        await act.Should().ThrowAsync<FormatException>();
    }
}
