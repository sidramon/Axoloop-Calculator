namespace Domain.Tests.Calculator;

using Domain.Calculator;
using Domain.Calculator.Values;
using Domain.Tests.Calculator.TestHelpers;
using FluentAssertions;
using Value = Domain.Calculator.Values.Value;

public class FunctionValueResolutionTests
{
    private readonly Domain.Calculator.Parsing.Parser _parser = ParserFactory.CreateDefault();
    private readonly FunctionContext _functionContext = new();
    private readonly Evaluator _evaluator;
    private readonly VariableContext _globals = new();

    public FunctionValueResolutionTests()
    {
        _evaluator = EvaluatorFactory.CreateDefault(_functionContext);
    }

    private Value Run(string input) => _evaluator.Evaluate(_parser.Parse(input), _globals);

    [Fact]
    public void BareUserFunctionIdentifier_ResolvesToFunctionValue()
    {
        Run("f(x) := x^2 + 1");

        var result = (FunctionValue)Run("f");

        result.Name.Should().Be("f");
        result.Arity.Should().Be(1);
        result.Signature.Should().Be("f(x)");
    }

    [Fact]
    public void VariableWithSameNameAsFunction_MasksTheFunction()
    {
        Run("f(x) := x^2");
        Run("f := 42");

        var result = (NumberValue)Run("f");

        result.Number.Should().BeApproximately(42, 1e-10);
    }

    [Fact]
    public void BareBuiltinIdentifier_ResolvesToFunctionValue()
    {
        var result = (FunctionValue)Run("sqrt");

        result.Name.Should().Be("sqrt");
        result.Arity.Should().Be(1);
    }

    [Fact]
    public void BuiltinStillCallableNormallyAfterBecomingAValue()
    {
        var result = (NumberValue)Run("sqrt(9)");

        result.Number.Should().BeApproximately(3, 1e-10);
    }

    [Fact]
    public void BareSpecialForm_Throws()
    {
        var act = () => Run("if");

        act.Should().Throw<InvalidOperationException>().WithMessage("*if*");
    }

    [Fact]
    public void FunctionValueInvoke_CallsUserFunctionBody()
    {
        Run("f(x) := 2*x + 1");
        var f = (FunctionValue)Run("f");

        var result = (NumberValue)f.Invoke(new Value[] { new NumberValue(3) });

        result.Number.Should().BeApproximately(7, 1e-10);
    }

    [Fact]
    public void FunctionValueInvoke_RespectsParameterMaskingAndDepthGuard()
    {
        Run("x := 100");
        Run("echoParam(x) := x");
        var f = (FunctionValue)Run("echoParam");

        var result = (NumberValue)f.Invoke(new Value[] { new NumberValue(1) });

        result.Number.Should().BeApproximately(1, 1e-10);
        ((NumberValue)Run("x")).Number.Should().BeApproximately(100, 1e-10);
    }

    [Fact]
    public void FunctionValueInvoke_WrongArgumentCount_Throws()
    {
        Run("f(x) := x");
        var f = (FunctionValue)Run("f");

        var act = () => f.Invoke(new Value[] { new NumberValue(1), new NumberValue(2) });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void FunctionValueInvoke_RecursionWithoutBaseCase_ThrowsOnceMaxCallDepthExceeded()
    {
        Run("loop(n) := loop(n)");
        var f = (FunctionValue)Run("loop");

        var act = () => f.Invoke(new Value[] { new NumberValue(0) });

        act.Should().Throw<InvalidOperationException>().WithMessage("*depth*");
    }

    [Fact]
    public void UndefinedIdentifier_StillThrowsUndefinedVariable()
    {
        var act = () => Run("nope");

        act.Should().Throw<InvalidOperationException>().WithMessage("*nope*");
    }
}
