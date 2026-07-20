namespace Domain.Tests.Calculator.Symbolic;

using Domain.Calculator;
using Domain.Calculator.Ast;
using Domain.Calculator.Operations;
using Domain.Calculator.Operations.Functions;
using Domain.Calculator.Operations.Functions.Scalar.Trigonometric;
using Domain.Calculator.Operations.SpecialForms;
using Domain.Calculator.Parsing;
using Domain.Calculator.Symbolic;
using Domain.Calculator.Values;
using FluentAssertions;

public class SymbolicPrinterTests
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

    private static Symbol A => new("a");
    private static Symbol B => new("b");
    private static Symbol C => new("c");
    private static Symbol X => new("x");
    private static Number N(int v) => new(new Rational(v));

    // --- Direct printing: parentheses driven by precedence ---

    [Fact]
    public void Print_SumInsideProduct_KeepsParens()
    {
        var expr = new Product(new SymbolicExpression[] { new Sum(new SymbolicExpression[] { A, B }), C });

        SymbolicPrinter.Print(expr).Should().Be("(a + b)*c");
    }

    [Fact]
    public void Print_ProductInsideSum_LosesParens()
    {
        var expr = new Sum(new SymbolicExpression[] { A, new Product(new SymbolicExpression[] { B, C }) });

        SymbolicPrinter.Print(expr).Should().Be("a + b*c");
    }

    [Fact]
    public void Print_NegativeOneTimesX_PrintsAsMinusX()
    {
        var expr = new Product(new SymbolicExpression[] { new Number(new Rational(-1)), X });

        SymbolicPrinter.Print(expr).Should().Be("-x");
    }

    [Fact]
    public void Print_PowerOfNegativeOne_PrintsAsReciprocal()
    {
        var expr = new Power(X, new Number(new Rational(-1)));

        SymbolicPrinter.Print(expr).Should().Be("1/x");
    }

    [Fact]
    public void Print_ProductWithReciprocalFactor_PrintsAsDivision()
    {
        var expr = new Product(new SymbolicExpression[] { A, new Power(B, new Number(new Rational(-1))) });

        SymbolicPrinter.Print(expr).Should().Be("a/b");
    }

    [Fact]
    public void Print_SumWithNegatedTerm_PrintsAsSubtraction()
    {
        var expr = new Sum(new SymbolicExpression[]
        {
            A,
            new Product(new SymbolicExpression[] { new Number(new Rational(-1)), B }),
        });

        SymbolicPrinter.Print(expr).Should().Be("a - b");
    }

    [Fact]
    public void Print_SubtractionOfSum_KeepsParensAroundNegatedSum()
    {
        // a - (b + c): must NOT print as "a - b + c", which is a - b + c (wrong value).
        var expr = new Sum(new SymbolicExpression[]
        {
            A,
            new Product(new SymbolicExpression[] { new Number(new Rational(-1)), new Sum(new SymbolicExpression[] { B, C }) }),
        });

        SymbolicPrinter.Print(expr).Should().Be("a - (b + c)");
    }

    [Fact]
    public void Print_NegativeIntegerBaseOfPower_KeepsParens()
    {
        var expr = new Power(new Number(new Rational(-2)), X);

        SymbolicPrinter.Print(expr).Should().Be("(-2)^x");
    }

    [Fact]
    public void Print_PositiveIntegerBaseOfPower_NoParens()
    {
        var expr = new Power(N(2), X);

        SymbolicPrinter.Print(expr).Should().Be("2^x");
    }

    [Fact]
    public void Print_PowerOfPower_KeepsParensOnBase()
    {
        var expr = new Power(new Power(A, B), C);

        SymbolicPrinter.Print(expr).Should().Be("(a^b)^c");
    }

    [Fact]
    public void Print_FractionalExponent_ParenthesizedOnPower()
    {
        var expr = new Power(X, new Number(new Rational(1, 2)));

        SymbolicPrinter.Print(expr).Should().Be("x^(1/2)");
    }

    [Fact]
    public void Print_FractionAsSumTerm_NoExtraParens()
    {
        var expr = new Sum(new SymbolicExpression[] { X, new Number(new Rational(1, 2)) });

        SymbolicPrinter.Print(expr).Should().Be("x + 1/2");
    }

    [Fact]
    public void Print_IntegerNumber_HasNoDenominator()
    {
        SymbolicPrinter.Print(N(5)).Should().Be("5");
    }

    [Fact]
    public void Print_Fraction_ShowsNumeratorSlashDenominator()
    {
        SymbolicPrinter.Print(new Number(new Rational(5, 3))).Should().Be("5/3");
    }

    [Fact]
    public void Print_ReciprocalOfProduct_ParenthesizesDenominator()
    {
        // 1/(a*b): two reciprocal factors, no numerator.
        var expr = new Product(new SymbolicExpression[]
        {
            new Power(A, new Number(new Rational(-1))),
            new Power(B, new Number(new Rational(-1))),
        });

        SymbolicPrinter.Print(expr).Should().Be("1/(a*b)");
    }

    [Fact]
    public void Print_DivisionByProduct_ParenthesizesDenominator()
    {
        // a/(b*c): must not print "a/b*c", which is (a/b)*c.
        var expr = new Product(new SymbolicExpression[]
        {
            A,
            new Power(B, new Number(new Rational(-1))),
            new Power(C, new Number(new Rational(-1))),
        });

        SymbolicPrinter.Print(expr).Should().Be("a/(b*c)");
    }

    [Fact]
    public void Print_FunctionCallArgument_NoUnnecessaryParens()
    {
        var expr = new FunctionCall("f", new SymbolicExpression[] { new Sum(new SymbolicExpression[] { A, B }) });

        SymbolicPrinter.Print(expr).Should().Be("f(a + b)");
    }

    [Fact]
    public void Print_ThreeTermSum_JoinsWithPlus()
    {
        var expr = new Sum(new SymbolicExpression[] { A, B, C });

        SymbolicPrinter.Print(expr).Should().Be("a + b + c");
    }

    // --- Round trip: print, reparse, reconvert, compare by evaluating both at several
    // points. This is the same "equivalent, not necessarily identical" bar the task sets
    // for the print/parse round trip, and the same technique the AST<->Symbolic round
    // trip already uses. A stricter structural-equality check is NOT used here: the
    // canonicalizer has a real, documented gap — it never distributes a power over a
    // product's factors (Power(Product(b,c), -1) vs Product(Power(b,-1), Power(c,-1))
    // are both stable fixed points for the same value, e.g. "a/(b*c)" vs "a/b/c" — so
    // print-then-reparse can legitimately land on the other, mathematically equal, form.
    // See the task summary for the full explanation.

    private static SymbolicExpression CanonicalOf(string source) =>
        Canonicalizer.Canonicalize(AstConverter.ToSymbolic(Parse(source)));

    private static double EvaluateSymbolic(SymbolicExpression expression, IReadOnlyDictionary<string, double> bindings)
    {
        var ast = AstConverter.ToAst(expression, Operators);
        var evaluator = new Evaluator(new IFunction[] { new SinFunction() }, Array.Empty<ISpecialForm>(), new FunctionContext());
        var context = new VariableContext();
        foreach (var (name, value) in bindings) context.Bind(name, new NumberValue(value));
        return ((NumberValue)evaluator.Evaluate(ast, context)).Number;
    }

    // All-positive and x always an integer: several test expressions use these as the
    // base of a power, or as the exponent of a fixed negative base ("(-2)^x"), and a
    // negative base raised to a non-integer exponent is NaN in the reals — a real domain
    // restriction, not something to work around with special-case NaN comparisons.
    private static readonly IReadOnlyList<IReadOnlyDictionary<string, double>> ProbePoints = new IReadOnlyDictionary<string, double>[]
    {
        new Dictionary<string, double> { ["a"] = 2, ["b"] = 3, ["c"] = 5, ["x"] = 2 },
        new Dictionary<string, double> { ["a"] = 1, ["b"] = 4, ["c"] = 2, ["x"] = 7 },
        new Dictionary<string, double> { ["a"] = 0.5, ["b"] = 3, ["c"] = 6, ["x"] = 4 },
    };

    private static void AssertRoundTrips(SymbolicExpression canonical)
    {
        var printed = SymbolicPrinter.Print(canonical);
        var reparsed = Parse(printed);
        var reconverted = Canonicalizer.Canonicalize(AstConverter.ToSymbolic(reparsed));

        foreach (var point in ProbePoints)
        {
            var expected = EvaluateSymbolic(canonical, point);
            var actual = EvaluateSymbolic(reconverted, point);
            actual.Should().BeApproximately(expected, 1e-9, because: $"'{printed}' should evaluate the same as the original");
        }
    }

    [Theory]
    [InlineData("a + b*c")]
    [InlineData("(a+b)*c")]
    [InlineData("a - b - c")]
    [InlineData("a - (b - c)")]
    [InlineData("-a*b")]
    [InlineData("-(a+b)")]
    [InlineData("a/b/c")]
    [InlineData("a/(b*c)")]
    [InlineData("(a/b)^2")]
    [InlineData("a^b^c")]
    [InlineData("(a^b)^c")]
    [InlineData("(-2)^x")]
    [InlineData("2*x^2 + 3*x - 5")]
    [InlineData("x*x*x")]
    [InlineData("x/x")]
    [InlineData("1/(a*b)")]
    [InlineData("(a+b)/(c+x)")]
    [InlineData("-x^2")]
    [InlineData("x^(1/2)")]
    [InlineData("sin(a+b)")]
    public void RoundTrip_PrintThenParse_ProducesEquivalentValue(string source)
    {
        AssertRoundTrips(CanonicalOf(source));
    }

    [Fact]
    public void RoundTrip_HandBuiltNegatedSum_PrintThenParse_ProducesEquivalentValue()
    {
        var expr = new Sum(new SymbolicExpression[]
        {
            A,
            new Product(new SymbolicExpression[] { new Number(new Rational(-1)), new Sum(new SymbolicExpression[] { B, C }) }),
        });

        AssertRoundTrips(expr);
    }

    [Fact]
    public void RoundTrip_HandBuiltNegativeBasePower_PrintThenParse_ProducesEquivalentTree()
    {
        var expr = new Power(new Number(new Rational(-2)), X);

        AssertRoundTrips(expr);
    }
}
