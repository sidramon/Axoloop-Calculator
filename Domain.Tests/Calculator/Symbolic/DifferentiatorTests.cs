namespace Domain.Tests.Calculator.Symbolic;

using Domain.Calculator;
using Domain.Calculator.Algorithms;
using Domain.Calculator.Ast;
using Domain.Calculator.Operations;
using Domain.Calculator.Operations.SpecialForms;
using Domain.Calculator.Parsing;
using Domain.Calculator.Symbolic;
using Domain.Calculator.Values;
using Domain.Tests.Calculator.TestHelpers;
using FluentAssertions;

public class DifferentiatorTests
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

    private static Symbol X => new("x");
    private static Symbol Y => new("y");
    private static Number N(int v) => new(new Rational(v));

    private static SymbolicExpression D(string source, string variable = "x") =>
        Differentiator.Differentiate(AstConverter.ToSymbolic(Parse(source)), variable);

    // ---- Structural: canonicalized result equals the expected tree exactly ----

    [Fact]
    public void Number_DerivativeIsZero()
    {
        Differentiator.Differentiate(N(5), "x").Should().Be(N(0));
    }

    [Fact]
    public void Symbol_DerivationVariable_IsOne()
    {
        Differentiator.Differentiate(X, "x").Should().Be(N(1));
    }

    [Fact]
    public void Symbol_OtherVariable_IsZero()
    {
        Differentiator.Differentiate(Y, "x").Should().Be(N(0));
    }

    [Fact]
    public void Sum_PolynomialPlusConstant_DifferentiatesTermwise()
    {
        // x^2 + 3x + 5 -> 2x + 3
        D("x^2 + 3*x + 5").Should().Be(new Sum(new SymbolicExpression[]
        {
            N(3),
            new Product(new SymbolicExpression[] { N(2), X }),
        }));
    }

    [Fact]
    public void Product_NAryThreeFactors_UsesGeneralizedProductRule()
    {
        // d/dx[x*y*z] = 1*y*z + x*0*z + x*y*0 = y*z
        D("x*y*z").Should().Be(new Product(new SymbolicExpression[] { Y, new Symbol("z") }));
    }

    [Fact]
    public void Power_ConstantExponent_UsesPowerRule()
    {
        // diff(3*x^4, x) -> 12*x^3
        D("3*x^4").Should().Be(new Product(new SymbolicExpression[] { N(12), new Power(X, N(3)) }));
    }

    [Fact]
    public void Power_ConstantBase_UsesExponentialRule()
    {
        // diff(2^x, x) -> 2^x * ln(2)
        D("2^x").Should().Be(new Product(new SymbolicExpression[]
        {
            new Power(N(2), X),
            new FunctionCall("ln", new SymbolicExpression[] { N(2) }),
        }));
    }

    [Fact]
    public void Power_BothBaseAndExponentVariable_UsesFullRule()
    {
        // diff(x^x, x) -> x^x * (ln(x) + 1)
        D("x^x").Should().Be(new Product(new SymbolicExpression[]
        {
            new Power(X, X),
            new Sum(new SymbolicExpression[] { N(1), new FunctionCall("ln", new SymbolicExpression[] { X }) }),
        }));
    }

    [Fact]
    public void Power_ReciprocalOfX_MatchesNegativeReciprocalSquare()
    {
        // diff(1/x, x) -> -1/x^2
        D("1/x").Should().Be(new Product(new SymbolicExpression[]
        {
            new Number(new Rational(-1)),
            new Power(X, new Number(new Rational(-2))),
        }));
    }

    [Fact]
    public void FunctionCall_Sin_ChainRuleGivesCos()
    {
        D("sin(x)").Should().Be(new FunctionCall("cos", new SymbolicExpression[] { X }));
    }

    [Fact]
    public void FunctionCall_ChainedTwoLevels_SinOfCos()
    {
        // d/dx sin(cos(x)) = cos(cos(x)) * -sin(x) = -sin(x)*cos(cos(x)); factors sort by
        // tier then, within the same FunctionCall tier, alphabetically by name ("cos" < "sin").
        D("sin(cos(x))").Should().Be(new Product(new SymbolicExpression[]
        {
            new Number(new Rational(-1)),
            new FunctionCall("cos", new SymbolicExpression[] { new FunctionCall("cos", new SymbolicExpression[] { X }) }),
            new FunctionCall("sin", new SymbolicExpression[] { X }),
        }));
    }

    [Fact]
    public void Product_XTimesSinX_UsesProductRule()
    {
        // d/dx[x*sin(x)] = sin(x) + x*cos(x); Sum terms sort by tier (Product before
        // FunctionCall), so the product term comes first.
        D("x*sin(x)").Should().Be(new Sum(new SymbolicExpression[]
        {
            new Product(new SymbolicExpression[] { X, new FunctionCall("cos", new SymbolicExpression[] { X }) }),
            new FunctionCall("sin", new SymbolicExpression[] { X }),
        }));
    }

    [Fact]
    public void FunctionCall_Ln_ChainRuleGivesReciprocal()
    {
        D("ln(x)").Should().Be(new Power(X, new Number(new Rational(-1))));
    }

    [Fact]
    public void FreeVariable_TreatedAsConstant_RegardlessOfName()
    {
        // diff(a*x^2, x) -> 2*a*x -- "a" is a free symbol, not looked up anywhere.
        D("a*x^2").Should().Be(new Product(new SymbolicExpression[] { N(2), new Symbol("a"), X }));
    }

    [Fact]
    public void ConstantExpression_DifferentiatedWithRespectToUnrelatedVariable_IsZero()
    {
        D("5", "x").Should().Be(N(0));
    }

    [Fact]
    public void NthDerivative_CubicOrderTwo_MatchesLinearResult()
    {
        // diff(x^3, x, 2) -> 6*x
        var symbolic = AstConverter.ToSymbolic(Parse("x^3"));
        Differentiator.DifferentiateNth(symbolic, "x", 2).Should().Be(new Product(new SymbolicExpression[] { N(6), X }));
    }

    [Fact]
    public void NthDerivative_OrderZero_Throws()
    {
        var symbolic = AstConverter.ToSymbolic(Parse("x^3"));
        var act = () => Differentiator.DifferentiateNth(symbolic, "x", 0);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void NthDerivative_ExceedsCap_Throws()
    {
        var symbolic = AstConverter.ToSymbolic(Parse("x^3"));
        var act = () => Differentiator.DifferentiateNth(symbolic, "x", 21);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void FunctionCall_NoKnownDerivative_ThrowsNamingTheFunction()
    {
        var symbolic = new FunctionCall("det", new SymbolicExpression[] { X });

        var act = () => Differentiator.Differentiate(symbolic, "x");

        act.Should().Throw<InvalidOperationException>().WithMessage("*det*");
    }

    [Fact]
    public void Abs_Derivative_IsXOverAbsX()
    {
        D("abs(x)").Should().Be(new Product(new SymbolicExpression[]
        {
            X,
            new Power(new FunctionCall("abs", new SymbolicExpression[] { X }), new Number(new Rational(-1))),
        }));
    }

    // ---- Numeric cross-validation against ndiff (finite differences) ----

    private static double EvaluateAt(IExpression expression, string variable, double value)
    {
        var evaluator = new Evaluator(EvaluatorFactory.Builtins(), Array.Empty<ISpecialForm>(), new FunctionContext());
        var context = new VariableContext();
        context.Bind(variable, new NumberValue(value));
        return ((NumberValue)evaluator.Evaluate(expression, context)).Number;
    }

    private static void AssertMatchesNumericDerivative(string source, string variable, params double[] points)
    {
        var ast = Parse(source);
        var symbolic = AstConverter.ToSymbolic(ast);
        var derivativeAst = AstConverter.ToAst(Differentiator.Differentiate(symbolic, variable), Operators);

        foreach (var x in points)
        {
            var numeric = NumericalCalculus.Derivative(v => EvaluateAt(ast, variable, v), x);
            var exact = EvaluateAt(derivativeAst, variable, x);
            exact.Should().BeApproximately(numeric, 1e-4, $"diff({source}, {variable}) should match ndiff at {variable}={x}");
        }
    }

    [Theory]
    [InlineData("3*x^4", 1.0, 2.0, -1.5)]
    [InlineData("x^2 + 3*x + 5", -2.0, 0.0, 4.0)]
    [InlineData("sin(x)", 0.3, 1.0, -1.0)]
    [InlineData("sin(x^2)", 0.5, 1.2, -0.8)]
    [InlineData("x*sin(x)", 0.5, 2.0, -1.0)]
    [InlineData("1/x", 0.5, 2.0, -3.0)]
    [InlineData("ln(x)", 0.5, 1.0, 3.0)]
    [InlineData("x^x", 1.0, 2.0, 0.5)]
    [InlineData("cos(x)", 0.2, 1.5, -0.5)]
    [InlineData("tan(x)", 0.2, 0.5, -0.3)]
    [InlineData("asin(x)", 0.2, -0.5, 0.6)]
    [InlineData("acos(x)", 0.2, -0.5, 0.6)]
    [InlineData("atan(x)", 0.5, 2.0, -1.5)]
    [InlineData("sqrt(x)", 1.0, 4.0, 9.0)]
    [InlineData("csc(x)", 0.5, 1.5, 2.5)]
    [InlineData("sec(x)", 0.5, 1.5, 2.5)]
    [InlineData("cot(x)", 0.5, 1.5, 2.5)]
    [InlineData("acsc(x)", 2.0, -2.0, 5.0)]
    [InlineData("asec(x)", 2.0, -2.0, 5.0)]
    [InlineData("acot(x)", 0.5, 2.0, -1.5)]
    [InlineData("log(x, 3)", 1.0, 5.0, 10.0)]
    [InlineData("2^x", 0.5, 1.0, -1.0)]
    [InlineData("x^3 * sin(x)", 0.5, 1.5, -1.0)]
    [InlineData("abs(x)", 2.0, -3.0, 0.5)]
    public void SymbolicDerivative_MatchesNumericDerivative_AtSeveralPoints(string source, params double[] points)
    {
        AssertMatchesNumericDerivative(source, "x", points);
    }

    [Fact]
    public void SymbolicDerivative_MatchesNumericDerivative_ForFreeVariableCase()
    {
        // diff(a*x^2, x) evaluated with "a" bound alongside x.
        var ast = Parse("a*x^2");
        var symbolic = AstConverter.ToSymbolic(ast);
        var derivativeAst = AstConverter.ToAst(Differentiator.Differentiate(symbolic, "x"), Operators);

        foreach (var (a, x) in new[] { (2.0, 1.0), (-1.0, 3.0), (0.5, -2.0) })
        {
            var evaluator = new Evaluator(EvaluatorFactory.Builtins(), Array.Empty<ISpecialForm>(), new FunctionContext());
            var context = new VariableContext();
            context.Bind("a", new NumberValue(a));

            double EvaluateWithX(double xValue)
            {
                context.Bind("x", new NumberValue(xValue));
                return ((NumberValue)evaluator.Evaluate(ast, context)).Number;
            }

            var numeric = NumericalCalculus.Derivative(EvaluateWithX, x);

            context.Bind("x", new NumberValue(x));
            var exact = ((NumberValue)evaluator.Evaluate(derivativeAst, context)).Number;

            exact.Should().BeApproximately(numeric, 1e-4);
        }
    }
}
