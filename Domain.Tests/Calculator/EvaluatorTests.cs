namespace Domain.Tests.Calculator;

using Domain.Calculator;
using Domain.Calculator.Ast;
using Domain.Calculator.Operations;
using Domain.Calculator.Values;
using Domain.Tests.Calculator.TestHelpers;
using FluentAssertions;

public class EvaluatorTests
{
    private readonly FunctionContext _functionContext = new();
    private readonly Evaluator _evaluator;

    public EvaluatorTests()
    {
        _evaluator = EvaluatorFactory.CreateDefault(_functionContext);
    }

    [Fact]
    public void Evaluate_DefinedVariable_ReturnsItsValue()
    {
        var context = new VariableContext();
        context.Set("x", new NumberValue(5));

        var result = _evaluator.Evaluate(new IdentifierExpression("x"), context);

        result.Should().Be(new NumberValue(5));
    }

    [Fact]
    public void Evaluate_UndefinedVariable_ThrowsInvalidOperationException()
    {
        var context = new VariableContext();

        var act = () => _evaluator.Evaluate(new IdentifierExpression("missing"), context);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*missing*");
    }

    [Fact]
    public void Evaluate_Assignment_ReturnsAssignedValueAndStoresIt()
    {
        var context = new VariableContext();
        var assignment = new AssignmentExpression("y", new NumberExpression(new NumberValue(10)));

        var result = _evaluator.Evaluate(assignment, context);

        result.Should().Be(new NumberValue(10));
        context.Get("y").Should().Be(new NumberValue(10));
    }

    [Fact]
    public void Evaluate_MatrixExpression_EvaluatesEachCellRecursively()
    {
        var context = new VariableContext();
        var matrix = new MatrixExpression(new IReadOnlyList<IExpression>[]
        {
            new IExpression[]
            {
                new BinaryExpression(
                    new NumberExpression(new NumberValue(1)),
                    new AddOperator(),
                    new NumberExpression(new NumberValue(1))),
                new NumberExpression(new NumberValue(3)),
            },
        });

        var result = (MatrixValue)_evaluator.Evaluate(matrix, context);

        result[0, 0].Should().BeApproximately(2, 1e-10);
        result[0, 1].Should().BeApproximately(3, 1e-10);
    }

    [Fact]
    public void Evaluate_MatrixExpressionWithNonNumericCell_ThrowsInvalidOperationException()
    {
        var context = new VariableContext();
        var matrix = new MatrixExpression(new IReadOnlyList<IExpression>[]
        {
            new IExpression[]
            {
                new BinaryExpression(
                    new NumberExpression(new NumberValue(1)),
                    new EqualsOperator(),
                    new NumberExpression(new NumberValue(1))),
            },
        });

        var act = () => _evaluator.Evaluate(matrix, context);

        act.Should().Throw<InvalidOperationException>();
    }

    // ---- Calls ----

    [Fact]
    public void Evaluate_CallToBuiltin_ReturnsResult()
    {
        var context = new VariableContext();
        var call = new CallExpression("abs", new IExpression[] { new NumberExpression(new NumberValue(-5)) });

        var result = (NumberValue)_evaluator.Evaluate(call, context);

        result.Number.Should().BeApproximately(5, 1e-10);
    }

    [Fact]
    public void Evaluate_CallToUndefinedFunction_ThrowsInvalidOperationExceptionMentioningName()
    {
        var context = new VariableContext();
        var call = new CallExpression("nope", Array.Empty<IExpression>());

        var act = () => _evaluator.Evaluate(call, context);

        act.Should().Throw<InvalidOperationException>().WithMessage("*nope*");
    }

    [Fact]
    public void Evaluate_CallWithWrongArity_ThrowsInvalidOperationException()
    {
        var context = new VariableContext();
        var call = new CallExpression("abs", new IExpression[]
        {
            new NumberExpression(new NumberValue(1)),
            new NumberExpression(new NumberValue(2)),
        });

        var act = () => _evaluator.Evaluate(call, context);

        act.Should().Throw<InvalidOperationException>();
    }

    // ---- Function definitions ----

    [Fact]
    public void Evaluate_FunctionDefinition_ReturnsFunctionDefinedValueAndRegistersFunction()
    {
        var context = new VariableContext();
        var definition = new FunctionDefinitionExpression(
            "double",
            new[] { "x" },
            new BinaryExpression(new IdentifierExpression("x"), new Domain.Calculator.Operations.MultiplyOperator(), new NumberExpression(new NumberValue(2))));

        var result = (FunctionDefinedValue)_evaluator.Evaluate(definition, context);

        result.Name.Should().Be("double");
        result.Parameters.Should().Equal("x");
        _functionContext.TryGet("double", out _).Should().BeTrue();
    }

    [Fact]
    public void Evaluate_RedefiningBuiltinFunction_ThrowsInvalidOperationException()
    {
        var context = new VariableContext();
        var definition = new FunctionDefinitionExpression("sqrt", new[] { "x" }, new NumberExpression(new NumberValue(5)));

        var act = () => _evaluator.Evaluate(definition, context);

        act.Should().Throw<InvalidOperationException>().WithMessage("*sqrt*");
    }

    [Fact]
    public void Evaluate_RedefiningSpecialForm_ThrowsInvalidOperationException()
    {
        var context = new VariableContext();
        var definition = new FunctionDefinitionExpression("if", new[] { "x" }, new NumberExpression(new NumberValue(5)));

        var act = () => _evaluator.Evaluate(definition, context);

        act.Should().Throw<InvalidOperationException>().WithMessage("*if*");
    }
}
