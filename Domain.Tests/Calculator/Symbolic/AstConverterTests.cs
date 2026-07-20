namespace Domain.Tests.Calculator.Symbolic;

using Domain.Calculator;
using Domain.Calculator.Ast;
using Domain.Calculator.Operations;
using Domain.Calculator.Parsing;
using Domain.Calculator.Symbolic;
using Domain.Calculator.Values;
using FluentAssertions;

public class AstConverterTests
{
    private static readonly IReadOnlyDictionary<string, IOperator> Operators = new IOperator[]
    {
        new AddOperator(), new SubtractOperator(), new MultiplyOperator(), new DivideOperator(), new PowerOperator(),
    }.ToDictionary(o => o.Symbol);

    private static Parser CreateParser() => new(
        Operators.Values,
        new IUnaryOperator[] { new FactorialOperator() },
        new IUnaryOperator[] { new NegateOperator() },
        new Tokenizer());

    private static IExpression Parse(string input) => CreateParser().Parse(input);

    // --- AST -> Symbolic: basic shapes ---

    [Fact]
    public void ToSymbolic_Addition_BecomesSum()
    {
        var result = AstConverter.ToSymbolic(Parse("a + b"));

        result.Should().Be(new Sum(new SymbolicExpression[] { new Symbol("a"), new Symbol("b") }));
    }

    [Fact]
    public void ToSymbolic_Subtraction_BecomesSumOfNegatedProduct()
    {
        var result = AstConverter.ToSymbolic(Parse("a - b"));

        result.Should().Be(new Sum(new SymbolicExpression[]
        {
            new Symbol("a"),
            new Product(new SymbolicExpression[] { new Number(new Rational(-1)), new Symbol("b") }),
        }));
    }

    [Fact]
    public void ToSymbolic_Division_BecomesProductWithNegativeOnePower()
    {
        var result = AstConverter.ToSymbolic(Parse("a / b"));

        result.Should().Be(new Product(new SymbolicExpression[]
        {
            new Symbol("a"),
            new Power(new Symbol("b"), new Number(new Rational(-1))),
        }));
    }

    [Fact]
    public void ToSymbolic_Power_BecomesPower()
    {
        var result = AstConverter.ToSymbolic(Parse("a ^ b"));

        result.Should().Be(new Power(new Symbol("a"), new Symbol("b")));
    }

    [Fact]
    public void ToSymbolic_UnaryMinus_BecomesNegativeOneProduct()
    {
        var result = AstConverter.ToSymbolic(Parse("-x"));

        result.Should().Be(new Product(new SymbolicExpression[] { new Number(new Rational(-1)), new Symbol("x") }));
    }

    [Fact]
    public void ToSymbolic_FunctionCall_BecomesFunctionCall()
    {
        var result = AstConverter.ToSymbolic(Parse("f(x, y)"));

        result.Should().Be(new FunctionCall("f", new SymbolicExpression[] { new Symbol("x"), new Symbol("y") }));
    }

    [Fact]
    public void ToSymbolic_Number_BecomesRational()
    {
        var result = AstConverter.ToSymbolic(Parse("5"));

        result.Should().Be(new Number(new Rational(5)));
    }

    // --- AST -> Symbolic: rejections ---

    [Fact]
    public void ToSymbolic_Assignment_Throws()
    {
        var act = () => AstConverter.ToSymbolic(new AssignmentExpression("x", new IdentifierExpression("y")));

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ToSymbolic_FunctionDefinition_Throws()
    {
        var act = () => AstConverter.ToSymbolic(
            new FunctionDefinitionExpression("f", new[] { "x" }, new IdentifierExpression("x")));

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ToSymbolic_Matrix_Throws()
    {
        var act = () => AstConverter.ToSymbolic(Parse("[1,2;3,4]"));

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ToSymbolic_Logical_Throws()
    {
        var act = () => AstConverter.ToSymbolic(new LogicalExpression(
            new IdentifierExpression("a"), LogicalOperator.And, new IdentifierExpression("b")));

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ToSymbolic_Not_Throws()
    {
        var act = () => AstConverter.ToSymbolic(new NotExpression(new IdentifierExpression("a")));

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ToSymbolic_ChainedCall_Throws()
    {
        var act = () => AstConverter.ToSymbolic(
            new InvokeExpression(new IdentifierExpression("f"), new IExpression[] { new IdentifierExpression("x") }));

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ToSymbolic_Comparison_Throws()
    {
        var act = () => AstConverter.ToSymbolic(new BinaryExpression(
            new IdentifierExpression("a"), new EqualsOperator(), new IdentifierExpression("b")));

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ToSymbolic_Factorial_Throws()
    {
        var act = () => AstConverter.ToSymbolic(new UnaryExpression(new IdentifierExpression("x"), new FactorialOperator()));

        act.Should().Throw<InvalidOperationException>();
    }

    // --- Round trip AST -> Symbolic -> AST, verified by numeric evaluation ---

    private static double Evaluate(IExpression expression, double x, double y)
    {
        var evaluator = new Evaluator(
            Array.Empty<Domain.Calculator.Operations.Functions.IFunction>(),
            Array.Empty<Domain.Calculator.Operations.SpecialForms.ISpecialForm>(),
            new FunctionContext());
        var context = new VariableContext();
        context.Bind("x", new NumberValue(x));
        context.Bind("y", new NumberValue(y));
        return ((NumberValue)evaluator.Evaluate(expression, context)).Number;
    }

    [Theory]
    [InlineData("x + y")]
    [InlineData("x - y")]
    [InlineData("x * y")]
    [InlineData("x / y")]
    [InlineData("x ^ 2")]
    [InlineData("2*x + 3*y - 5")]
    [InlineData("(x + y) * (x - y)")]
    [InlineData("x / y / 2")]
    [InlineData("-x + y")]
    [InlineData("x^2 - y^2")]
    public void RoundTrip_AstToSymbolicToAst_PreservesValueAtSeveralPoints(string source)
    {
        var originalAst = Parse(source);
        var symbolic = AstConverter.ToSymbolic(originalAst);
        var reconstructedAst = AstConverter.ToAst(symbolic, Operators);

        foreach (var (x, y) in new[] { (2.0, 3.0), (-1.5, 4.0), (0.0, 7.0), (10.0, -2.0) })
        {
            var originalValue = Evaluate(originalAst, x, y);
            var reconstructedValue = Evaluate(reconstructedAst, x, y);
            reconstructedValue.Should().BeApproximately(originalValue, 1e-9);
        }
    }
}
