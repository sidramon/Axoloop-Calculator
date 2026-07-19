namespace Domain.Tests.Calculator;

using Domain.Calculator;
using Domain.Calculator.Values;
using Domain.Tests.Calculator.TestHelpers;
using FluentAssertions;
using Value = Domain.Calculator.Values.Value;

public class UserFunctionEvaluationTests
{
    private readonly Domain.Calculator.Parsing.Parser _parser = ParserFactory.CreateDefault();
    private readonly FunctionContext _functionContext = new();
    private readonly Evaluator _evaluator;
    private readonly VariableContext _globals = new();

    public UserFunctionEvaluationTests()
    {
        _evaluator = EvaluatorFactory.CreateDefault(_functionContext);
    }

    private Value Run(string input, VariableContext? context = null) =>
        _evaluator.Evaluate(_parser.Parse(input), context ?? _globals);

    private double RunNumber(string input, VariableContext? context = null) =>
        ((NumberValue)Run(input, context)).Number;

    [Fact]
    public void Define_ZeroParameterFunction_ThenCall_ReturnsBodyValue()
    {
        Run("k() := 42");

        RunNumber("k()").Should().BeApproximately(42, 1e-10);
    }

    [Fact]
    public void Define_SingleParameterFunction_ThenCall_SubstitutesArgument()
    {
        Run("y(x) := 2*x + 5");

        RunNumber("y(3)").Should().BeApproximately(11, 1e-10);
    }

    [Fact]
    public void Define_MultiParameterFunction_ThenCall_UsesBuiltinInBody()
    {
        Run("f(a,b) := sqrt(a^2 + b^2)");

        RunNumber("f(3,4)").Should().BeApproximately(5, 1e-10);
    }

    [Fact]
    public void Define_FunctionUsingMatrixOperations_ThenCall_ReturnsMatrix()
    {
        Run("g(m) := transpose(m) * m");

        var result = (MatrixValue)Run("g([1,2;3,4])");

        result.Rows.Should().Be(2);
        result.Columns.Should().Be(2);
    }

    [Fact]
    public void Define_RecursiveFunctionWithIfBaseCase_ComputesFactorial()
    {
        Run("fact(n) := if(n <= 1, 1, n * fact(n-1))");

        RunNumber("fact(5)").Should().BeApproximately(120, 1e-10);
    }

    [Fact]
    public void Define_NestedIfInsideFunction_SelectsCorrectBranch()
    {
        Run("sign(x) := if(x > 0, 1, if(x < 0, -1, 0))");

        RunNumber("sign(-4)").Should().BeApproximately(-1, 1e-10);
    }

    [Fact]
    public void ParameterMasksGlobalVariable_WithoutOverwritingGlobal()
    {
        Run("x := 100");
        Run("echoParam(x) := x");

        RunNumber("echoParam(1)").Should().BeApproximately(1, 1e-10);
        RunNumber("x").Should().BeApproximately(100, 1e-10);
    }

    [Fact]
    public void FunctionSeesGlobalVariableAtCallTimeNotDefinitionTime_DynamicScoping()
    {
        Run("g := 1");
        Run("readGlobal() := g");

        RunNumber("readGlobal()").Should().BeApproximately(1, 1e-10);

        Run("g := 2");
        RunNumber("readGlobal()").Should().BeApproximately(2, 1e-10);
    }

    [Fact]
    public void CallingUndefinedFunction_ThrowsInvalidOperationException()
    {
        var act = () => Run("z(2)");

        act.Should().Throw<InvalidOperationException>().WithMessage("*z*");
    }

    [Fact]
    public void RedefiningBuiltinFunction_ThrowsInvalidOperationException()
    {
        var act = () => Run("sqrt(x) := 5");

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RedefiningSpecialForm_ThrowsInvalidOperationException()
    {
        var act = () => Run("if(x) := 5");

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void DefiningFunction_EchoesDefinitionSignature()
    {
        var result = (FunctionDefinedValue)Run("y(x) := 2*x + 5");

        result.Name.Should().Be("y");
        result.Parameters.Should().Equal("x");
    }

    [Fact]
    public void CallWithWrongArgumentCount_ThrowsInvalidOperationException()
    {
        Run("y(x) := x");

        var act = () => Run("y(1,2)");

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RecursionWithoutBaseCase_ThrowsOnceMaxCallDepthExceeded()
    {
        Run("loop(n) := loop(n)");

        var act = () => Run("loop(0)");

        act.Should().Throw<InvalidOperationException>().WithMessage("*depth*");
    }

    [Fact]
    public void RedefiningAUserFunction_IsAllowed()
    {
        Run("k() := 1");
        Run("k() := 2");

        RunNumber("k()").Should().BeApproximately(2, 1e-10);
    }
}
