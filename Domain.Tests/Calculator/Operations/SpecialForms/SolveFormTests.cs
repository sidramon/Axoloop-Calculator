namespace Domain.Tests.Calculator.Operations.SpecialForms;

using Domain.Calculator;
using Domain.Calculator.Values;
using Domain.Tests.Calculator.TestHelpers;
using FluentAssertions;
using Value = Domain.Calculator.Values.Value;

public class SolveFormTests
{
    private readonly Domain.Calculator.Parsing.Parser _parser = ParserFactory.CreateDefault();
    private readonly FunctionContext _functionContext = new();
    private readonly Evaluator _evaluator;
    private readonly VariableContext _globals = new();

    public SolveFormTests()
    {
        _evaluator = EvaluatorFactory.CreateDefault(_functionContext);
        _globals.Seed(Constants.All);
    }

    private Value Run(string input) => _evaluator.Evaluate(_parser.Parse(input), _globals);

    private SolutionValue RunSolution(string input) => (SolutionValue)Run(input);

    private double RunSingleRoot(string input)
    {
        var solution = RunSolution(input);
        solution.Values.Should().ContainSingle();
        return solution.Values[0];
    }

    [Fact]
    public void LinearEquation_RootVerifiedBySubstitution()
    {
        var root = RunSingleRoot("solve(3*x = 5, x)");

        root.Should().BeApproximately(5.0 / 3.0, 1e-4);
        (3 * root).Should().BeApproximately(5, 1e-3);
    }

    [Fact]
    public void QuadraticEquation_ReturnsTwoRootsSortedAscending()
    {
        var solution = RunSolution("solve(x^2 = 4, x)");

        solution.Unknown.Should().Be("x");
        solution.Values.Should().HaveCount(2);
        solution.TotalFound.Should().Be(2);
        solution.Values[0].Should().BeApproximately(-2, 1e-3);
        solution.Values[1].Should().BeApproximately(2, 1e-3);
    }

    [Fact]
    public void NoRealRoot_ThrowsRatherThanReturningNaN()
    {
        var act = () => Run("solve(x^2 + 1 = 0, x)");

        act.Should().Throw<InvalidOperationException>().WithMessage("*no solution*");
    }

    [Fact]
    public void UserFunctionInsideEquation_IsResolvedNormally()
    {
        Run("f(x) := 3*x + 5");

        var root = RunSingleRoot("solve(f(x) = 10, x)");

        root.Should().BeApproximately(5.0 / 3.0, 1e-4);
    }

    [Fact]
    public void GlobalVariableInsideEquation_IsNotConfusedWithTheUnknown()
    {
        Run("a := 2");

        var root = RunSingleRoot("solve(a*x = 6, x)");

        root.Should().BeApproximately(3, 1e-4);
    }

    [Fact]
    public void UnknownMasksGlobalVariableOfSameName_ButCallerContextIsRestoredAfter()
    {
        Run("x := 100");

        var root = RunSingleRoot("solve(3*x = 5, x)");

        root.Should().BeApproximately(5.0 / 3.0, 1e-4);
        ((NumberValue)Run("x")).Number.Should().BeApproximately(100, 1e-9);
    }

    [Fact]
    public void ImplicitZeroForm_IsEquivalentToExplicitEqualsZero()
    {
        var implicitRoot = RunSingleRoot("solve(3*x - 5, x)");
        var explicitRoot = RunSingleRoot("solve(3*x - 5 = 0, x)");

        implicitRoot.Should().BeApproximately(explicitRoot, 1e-9);
        implicitRoot.Should().BeApproximately(5.0 / 3.0, 1e-4);
    }

    [Fact]
    public void EvaluationPointThrowing_IsTreatedAsUndefinedAndResolutionContinues()
    {
        Run("g(x) := ln(x)");

        // ln is undefined for x <= 0 across most of the default domain; the root at x = 1
        // (ln(1) = 0) must still be found despite those failures.
        var root = RunSingleRoot("solve(g(x) = 0, x)");

        root.Should().BeApproximately(1, 1e-3);
    }

    [Fact]
    public void AsymptoteAtTheUnknown_DoesNotProduceAFalseRoot()
    {
        var act = () => Run("solve(1/x = 0, x)");

        act.Should().Throw<InvalidOperationException>().WithMessage("*no solution*");
    }

    [Fact]
    public void ExplicitDomain_IsRespected()
    {
        var root = RunSingleRoot("solve(x^2 = 4, x, 0, 100)");

        root.Should().BeApproximately(2, 1e-3);
    }

    [Fact]
    public void ExplicitDomain_ExcludingTheRoot_Throws()
    {
        var act = () => Run("solve(x^2 = 4, x, 10, 100)");

        act.Should().Throw<InvalidOperationException>().WithMessage("*no solution*");
    }

    [Fact]
    public void SecondArgumentNotAnIdentifier_Throws()
    {
        var act = () => Run("solve(3*x = 5, 2)");

        act.Should().Throw<InvalidOperationException>().WithMessage("*unknown*");
    }

    [Fact]
    public void ChainedComparisonAtTopLevel_Throws()
    {
        var act = () => Run("solve(3*x < 5, x)");

        act.Should().Throw<InvalidOperationException>().WithMessage("*equality*");
    }

    [Fact]
    public void ProtectedConstantAsUnknown_Throws()
    {
        var act = () => Run("solve(3*_pi = 5, _pi)");

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void BareSolveIdentifier_Throws()
    {
        var act = () => Run("solve");

        act.Should().Throw<InvalidOperationException>().WithMessage("*special form*");
    }

    [Fact]
    public void WrongArgumentCount_ThrowsMentioningValidArities()
    {
        var act = () => Run("solve(3*x = 5, x, 1)");

        act.Should().Throw<InvalidOperationException>().WithMessage("*2*4*");
    }

    [Fact]
    public void ManyRootsOverDefaultDomain_CapsReturnedValuesButReportsTrueTotal()
    {
        // sin(x) = 0.5 has roughly 63 roots across the default [-100, 100] domain.
        var solution = RunSolution("solve(sin(x) = 0.5, x)");

        solution.Values.Should().HaveCount(10);
        solution.TotalFound.Should().BeGreaterThan(10);
        solution.Values.Should().BeInAscendingOrder();
    }

    [Fact]
    public void FewRootsOverDefaultDomain_ReturnsAllOfThemWithTotalFoundMatching()
    {
        var solution = RunSolution("solve(x^2 = 4, x)");

        solution.TotalFound.Should().Be(solution.Values.Count);
    }
}
