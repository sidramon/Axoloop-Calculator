namespace Domain.Tests.Calculator.Operations.SpecialForms;

using Domain.Calculator;
using Domain.Calculator.Symbolic;
using Domain.Calculator.Values;
using Domain.Tests.Calculator.TestHelpers;
using FluentAssertions;
using Value = Domain.Calculator.Values.Value;

public class DiffFormTests
{
    private readonly Domain.Calculator.Parsing.Parser _parser = ParserFactory.CreateDefault();
    private readonly FunctionContext _functionContext = new();
    private readonly Evaluator _evaluator;
    private readonly VariableContext _globals = new();

    public DiffFormTests()
    {
        _evaluator = EvaluatorFactory.CreateDefault(_functionContext);
        _globals.Seed(Constants.All);
    }

    private Value Run(string input) => _evaluator.Evaluate(_parser.Parse(input), _globals);

    private string RunPrinted(string input) => SymbolicPrinter.Print(((SymbolicValue)Run(input)).Expression);

    [Fact]
    public void Polynomial_MatchesTaskExample()
    {
        RunPrinted("diff(3*x^4, x)").Should().Be("12*x^3");
    }

    [Fact]
    public void SumOfTerms_DifferentiatesTermwise()
    {
        // Canonical sort puts numbers before products: "3 + 2*x", not "2*x + 3".
        RunPrinted("diff(x^2 + 3*x + 5, x)").Should().Be("3 + 2*x");
    }

    [Fact]
    public void ProductOfSinAndX_UsesProductRule()
    {
        // Canonical sort puts products before function calls.
        RunPrinted("diff(x*sin(x), x)").Should().Be("x*cos(x) + sin(x)");
    }

    [Fact]
    public void Reciprocal_MatchesTaskExample()
    {
        RunPrinted("diff(1/x, x)").Should().Be("-1/x^2");
    }

    [Fact]
    public void Ln_MatchesTaskExample()
    {
        RunPrinted("diff(ln(x), x)").Should().Be("1/x");
    }

    [Fact]
    public void XToTheX_MatchesTaskExample()
    {
        // Canonical sort puts numbers before function calls inside the sum too.
        RunPrinted("diff(x^x, x)").Should().Be("x^x*(1 + ln(x))");
    }

    [Fact]
    public void FreeVariable_TreatedAsConstant_MatchesTaskExample()
    {
        RunPrinted("diff(a*x^2, x)").Should().Be("2*a*x");
    }

    [Fact]
    public void FreeVariable_IgnoresItsCurrentValue_EvenWhenAssigned()
    {
        // The whole point of never consulting VariableContext: a := 3 must not turn the
        // free variable "a" into the number 3 in the result.
        Run("a := 3");

        RunPrinted("diff(a*x, x)").Should().Be("a");
    }

    [Fact]
    public void ConstantExpression_CollapsesToNumberValue_NotSymbolicValue()
    {
        var result = Run("diff(5, x)");

        result.Should().BeOfType<NumberValue>();
        ((NumberValue)result).Number.Should().Be(0);
    }

    [Fact]
    public void NthDerivative_CubicOrderTwo_MatchesTaskExample()
    {
        RunPrinted("diff(x^3, x, 2)").Should().Be("6*x");
    }

    [Fact]
    public void NoKnownDerivative_ThrowsNamingTheFunction()
    {
        var act = () => Run("diff(det(m), x)");

        act.Should().Throw<InvalidOperationException>().WithMessage("*det*");
    }

    [Fact]
    public void SecondArgumentNotAnIdentifier_ThrowsClearError()
    {
        var act = () => Run("diff(x^2, 2)");

        act.Should().Throw<InvalidOperationException>().WithMessage("*second argument must name the variable*");
    }

    [Fact]
    public void SecondArgumentIsProtectedConstant_Throws()
    {
        var act = () => Run("diff(x^2, _pi)");

        act.Should().Throw<InvalidOperationException>().WithMessage("*protected*");
    }

    [Fact]
    public void MatrixArgument_RejectedWithClearMessage()
    {
        var act = () => Run("diff([1,2;3,4], x)");

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void UserDefinedFunction_DifferentiatesByItsOwnParameter()
    {
        Run("f(t) := t^2");

        RunPrinted("diff(f, t)").Should().Be("2*t");
    }

    [Fact]
    public void UserDefinedFunction_MismatchedVariableName_ThrowsNamingTheRealParameter()
    {
        Run("f(t) := t^2");

        var act = () => Run("diff(f, x)");

        act.Should().Throw<InvalidOperationException>().WithMessage("*'t'*");
    }

    [Fact]
    public void UserDefinedFunction_MultipleParameters_GivesPartialDerivative()
    {
        Run("g(a, b) := a*b^2");

        RunPrinted("diff(g, b)").Should().Be("2*a*b");
    }

    [Fact]
    public void SymbolicValue_AsArithmeticOperand_Throws()
    {
        var act = () => Run("diff(3*x^4, x) + 1");

        act.Should().Throw<InvalidOperationException>().WithMessage("*symbolic*");
    }

    [Fact]
    public void Ndiff_StillNumeric_Unaffected()
    {
        Run("f(x) := x^2");

        var result = (NumberValue)Run("ndiff(f, 3)");

        result.Number.Should().BeApproximately(6, 1e-4);
    }
}
