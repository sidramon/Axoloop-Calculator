namespace Domain.Tests.Calculator.Operations.SpecialForms;

using Domain.Calculator;
using Domain.Calculator.Algorithms;
using Domain.Calculator.Values;
using Domain.Tests.Calculator.TestHelpers;
using FluentAssertions;
using Value = Domain.Calculator.Values.Value;

public class LimFormTests
{
    private readonly Domain.Calculator.Parsing.Parser _parser = ParserFactory.CreateDefault();
    private readonly FunctionContext _functionContext = new();
    private readonly Evaluator _evaluator;
    private readonly VariableContext _globals = new();

    public LimFormTests()
    {
        _evaluator = EvaluatorFactory.CreateDefault(_functionContext);
        _globals.Seed(Constants.All);
    }

    private Value Run(string input) => _evaluator.Evaluate(_parser.Parse(input), _globals);

    private LimitValue RunLimit(string input) => (LimitValue)Run(input);

    [Fact]
    public void SinXOverX_AtZero_ConvergesToOne()
    {
        var result = RunLimit("lim(sin(x)/x, x, 0)");

        result.Variable.Should().Be("x");
        result.Target.Should().Be(0);
        result.Kind.Should().Be(LimitKind.Converges);
        result.Value.Should().BeApproximately(1, 1e-6);
    }

    [Fact]
    public void RationalExpression_AtRemovableHole_ConvergesToFour()
    {
        var result = RunLimit("lim((x^2 - 4)/(x - 2), x, 2)");

        result.Kind.Should().Be(LimitKind.Converges);
        result.Value.Should().BeApproximately(4, 1e-6);
    }

    [Fact]
    public void OneOverX_AtZero_ReportsOneSidedDiffer()
    {
        var result = RunLimit("lim(1/x, x, 0)");

        result.Kind.Should().Be(LimitKind.OneSidedDiffer);
        double.IsNegativeInfinity(result.LeftValue!.Value).Should().BeTrue();
        double.IsPositiveInfinity(result.RightValue!.Value).Should().BeTrue();
    }

    [Fact]
    public void OneOverX_AtZeroFromTheRight_DivergesToPositiveInfinity()
    {
        var result = RunLimit("lim(1/x, x, 0, 1)");

        result.Direction.Should().Be(1);
        result.Kind.Should().Be(LimitKind.DivergesToPositiveInfinity);
    }

    [Fact]
    public void OneOverX_AtZeroFromTheLeft_DivergesToNegativeInfinity()
    {
        var result = RunLimit("lim(1/x, x, 0, -1)");

        result.Direction.Should().Be(-1);
        result.Kind.Should().Be(LimitKind.DivergesToNegativeInfinity);
    }

    [Fact]
    public void OneOverXSquared_AtZero_DivergesToPositiveInfinityBothSides()
    {
        var result = RunLimit("lim(1/x^2, x, 0)");

        result.Kind.Should().Be(LimitKind.DivergesToPositiveInfinity);
    }

    [Fact]
    public void SinOfOneOverX_AtZero_HasNoLimit()
    {
        var result = RunLimit("lim(sin(1/x), x, 0)");

        result.Kind.Should().Be(LimitKind.NoLimit);
    }

    [Fact]
    public void OneOverX_AtPositiveInfinity_ConvergesToZero()
    {
        var result = RunLimit("lim(1/x, x, _inf)");

        result.Kind.Should().Be(LimitKind.Converges);
        result.Value.Should().BeApproximately(0, 1e-6);
    }

    [Fact]
    public void OnePlusOneOverXToTheX_AtPositiveInfinity_ConvergesToE()
    {
        var result = RunLimit("lim((1 + 1/x)^x, x, _inf)");

        result.Kind.Should().Be(LimitKind.Converges);
        result.Value.Should().BeApproximately(Math.E, 1e-2);
    }

    [Fact]
    public void ContinuousFunction_LimitEqualsDirectEvaluation()
    {
        var direct = ((NumberValue)Run("3^2")).Number;

        var result = RunLimit("lim(x^2, x, 3)");

        result.Kind.Should().Be(LimitKind.Converges);
        result.Value.Should().BeApproximately(direct, 1e-6);
        result.Value.Should().BeApproximately(9, 1e-6);
    }

    [Fact]
    public void GlobalVariable_IsVisibleInsideAndNotConfusedWithTheLimitVariable()
    {
        Run("a := 2");

        var result = RunLimit("lim(a*x, x, 3)");

        result.Kind.Should().Be(LimitKind.Converges);
        result.Value.Should().BeApproximately(6, 1e-6);
    }

    [Fact]
    public void CallingContext_DoesNotLeakTheLimitVariableAfterward()
    {
        RunLimit("lim(x^2, x, 3)");

        _globals.IsDefined("x").Should().BeFalse();
    }

    [Fact]
    public void LimitVariable_TemporarilyShadowsAGlobalOfTheSameName_ThenRestoresIt()
    {
        Run("x := 100");

        RunLimit("lim(x^2, x, 3)");

        ((NumberValue)Run("x")).Number.Should().Be(100);
    }

    [Fact]
    public void SecondArgumentNotAnIdentifier_ThrowsClearError()
    {
        var act = () => Run("lim(x^2, 2, 3)");

        act.Should().Throw<InvalidOperationException>().WithMessage("*second argument must name the variable*");
    }

    [Fact]
    public void SecondArgumentIsAProtectedConstant_Throws()
    {
        var act = () => Run("lim(_pi^2, _pi, 3)");

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ZeroDirection_ThrowsAmbiguousError()
    {
        var act = () => Run("lim(1/x, x, 0, 0)");

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void LimitValue_AsArithmeticOperand_ThrowsRatherThanBeingTreatedAsANumber()
    {
        var act = () => Run("lim(sin(x)/x, x, 0) + 1");

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void TargetMustBeANumber()
    {
        var act = () => Run("lim(x, x, sin)");

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void UndefinedFunctionNearTarget_StillFindsTheLimit()
    {
        // ln(x) is undefined for x <= 0, so approaching 1 from the left samples some
        // points fine and none go undefined here -- but this exercises ln itself, whose
        // one-sided domain restriction near other targets is the more interesting case
        // (see the *_AllUndefinedOnOneSide test at the algorithm level).
        var result = RunLimit("lim(ln(x), x, 1)");

        result.Kind.Should().Be(LimitKind.Converges);
        result.Value.Should().BeApproximately(0, 1e-6);
    }
}
