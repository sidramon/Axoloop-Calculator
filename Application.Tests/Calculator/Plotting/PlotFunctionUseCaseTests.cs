namespace Application.Tests.Calculator.Plotting;

using Application.Calculator.Plotting;
using Domain.Calculator;
using Domain.Calculator.Algorithms;
using Domain.Calculator.Ast;
using Domain.Calculator.Operations;
using Domain.Calculator.Operations.Functions;
using Domain.Calculator.Operations.Functions.Scalar;
using Domain.Calculator.Operations.SpecialForms;
using Domain.Calculator.Values;
using FluentAssertions;
using Value = Domain.Calculator.Values.Value;

public class PlotFunctionUseCaseTests
{
    private static readonly PlotFunctionUseCase UseCase = new();

    private static (Evaluator Evaluator, FunctionContext Functions, VariableContext Globals) CreateHarness(
        IEnumerable<IFunction>? builtins = null, IEnumerable<ISpecialForm>? specialForms = null)
    {
        var functions = new FunctionContext();
        var evaluator = new Evaluator(
            builtins ?? Array.Empty<IFunction>(),
            specialForms ?? Array.Empty<ISpecialForm>(),
            functions);
        return (evaluator, functions, new VariableContext());
    }

    private static void DefineFunction(FunctionContext functions, string name, string parameter, IExpression body) =>
        functions.Define(new UserFunction(name, new[] { parameter }, body));

    private static FunctionValue Resolve(Evaluator evaluator, VariableContext globals, string name) =>
        (FunctionValue)evaluator.Evaluate(new IdentifierExpression(name), globals);

    private static IExpression Square(string variable) => new BinaryExpression(
        new IdentifierExpression(variable), new PowerOperator(), new NumberExpression(new NumberValue(2)));

    private static IExpression Reciprocal(string variable) => new BinaryExpression(
        new NumberExpression(new NumberValue(1)), new DivideOperator(), new IdentifierExpression(variable));

    [Fact]
    public void Sample_ContinuousFunction_ProducesRequestedPointCountWithinBounds()
    {
        var (evaluator, functions, globals) = CreateHarness();
        DefineFunction(functions, "f", "x", Square("x"));
        var function = Resolve(evaluator, globals, "f");

        var series = UseCase.Sample(function, -5, 5, new PlotSampleRequest(SampleCount: 50));

        series.Points.Should().HaveCount(50);
        series.Points[0].X.Should().BeApproximately(-5, 1e-9);
        series.Points[^1].X.Should().BeApproximately(5, 1e-9);
        series.Points.Should().OnlyContain(p => p.Y.HasValue);
        series.XMin.Should().Be(-5);
        series.XMax.Should().Be(5);
    }

    [Fact]
    public void Sample_ParabolaOverSymmetricDomain_ComputesSingleMinimumExtremum()
    {
        var (evaluator, functions, globals) = CreateHarness();
        DefineFunction(functions, "f", "x", Square("x"));
        var function = Resolve(evaluator, globals, "f");

        var series = UseCase.Sample(function, -5, 5, new PlotSampleRequest(SampleCount: 41));

        series.Extrema.Should().ContainSingle();
        series.Extrema[0].Kind.Should().Be(ExtremumKind.Minimum);
        series.Extrema[0].X.Should().BeApproximately(0, 1e-6);
    }

    [Fact]
    public void Sample_FunctionThrowingOnPartOfDomain_ProducesGapsWithoutFailingWholePlot()
    {
        var (evaluator, functions, globals) = CreateHarness(new[] { new LnFunction() });
        DefineFunction(functions, "f", "x",
            new CallExpression("ln", new IExpression[] { new IdentifierExpression("x") }));
        var function = Resolve(evaluator, globals, "f");

        var series = UseCase.Sample(function, -5, 5, new PlotSampleRequest(SampleCount: 101));

        series.Points.Should().Contain(p => p.X < 0 && p.Y == null);
        series.Points.Should().Contain(p => p.X > 0 && p.Y.HasValue);
    }

    [Fact]
    public void Sample_AsymptoteWithoutException_ProducesGapNearThePoleButKeepsBothBranches()
    {
        var (evaluator, functions, globals) = CreateHarness();
        DefineFunction(functions, "f", "x", Reciprocal("x"));
        var function = Resolve(evaluator, globals, "f");

        // Domain [-1, 1] with an even sample count never lands exactly on x = 0, so any gap
        // here must come from the asymptote heuristic, not from a DivideByZeroException.
        var series = UseCase.Sample(function, -1, 1, new PlotSampleRequest(
            SampleCount: 100,
            YBounds: (-10, 10),
            MagnitudeMultiplier: 2));

        series.Points.Should().Contain(p => p.Y == null);
        series.Points.Should().Contain(p => p.X < -0.1 && p.Y.HasValue);
        series.Points.Should().Contain(p => p.X > 0.1 && p.Y.HasValue);
    }

    [Fact]
    public void Sample_IsolatedOutlier_DoesNotBlowOutTheYWindow()
    {
        var (evaluator, functions, globals) = CreateHarness(specialForms: new ISpecialForm[] { new IfForm() });
        var body = new CallExpression("if", new IExpression[]
        {
            new BinaryExpression(
                new IdentifierExpression("x"), new EqualsOperator(), new NumberExpression(new NumberValue(0))),
            new NumberExpression(new NumberValue(1_000_000)),
            new IdentifierExpression("x"),
        });
        DefineFunction(functions, "f", "x", body);
        var function = Resolve(evaluator, globals, "f");

        // 101 points over [-5, 5] hits x = 0 exactly once: a single outlier among 100 normal values.
        var series = UseCase.Sample(function, -5, 5, new PlotSampleRequest(SampleCount: 101));

        series.YMax.Should().BeLessThan(100);
    }

    [Fact]
    public void Sample_Oversample_WidensDomainByConfiguredFactor()
    {
        var (evaluator, functions, globals) = CreateHarness();
        DefineFunction(functions, "f", "x", new IdentifierExpression("x"));
        var function = Resolve(evaluator, globals, "f");

        var series = UseCase.Sample(
            function, -1, 1, new PlotSampleRequest(SampleCount: 40, Oversample: true, OversampleFactor: 4));

        series.XMin.Should().BeApproximately(-4, 1e-9);
        series.XMax.Should().BeApproximately(4, 1e-9);
    }

    [Fact]
    public void Sample_FunctionWithWrongArity_ThrowsClearError()
    {
        var (evaluator, functions, globals) = CreateHarness();
        functions.Define(new UserFunction("f", new[] { "a", "b" }, new NumberExpression(new NumberValue(0))));
        var function = Resolve(evaluator, globals, "f");

        var act = () => UseCase.Sample(function, -5, 5, new PlotSampleRequest(SampleCount: 10));

        act.Should().Throw<InvalidOperationException>().WithMessage("*one parameter*");
    }

    [Fact]
    public void Sample_InvalidDomain_ThrowsClearError()
    {
        var (evaluator, functions, globals) = CreateHarness();
        DefineFunction(functions, "f", "x", new IdentifierExpression("x"));
        var function = Resolve(evaluator, globals, "f");

        var act = () => UseCase.Sample(function, 5, -5, new PlotSampleRequest(SampleCount: 10));

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Sample_DerivativeOfCubicFunction_MatchesAnalyticDerivativeAtEveryPoint()
    {
        // Verifies the case that motivates deriv(f) returning a FunctionValue at all:
        // plotting a derivative without a name registered in FunctionContext.
        var (evaluator, functions, globals) = CreateHarness();
        DefineFunction(functions, "f", "x", new BinaryExpression(
            new IdentifierExpression("x"), new PowerOperator(), new NumberExpression(new NumberValue(3))));
        var f = Resolve(evaluator, globals, "f");

        var derivative = (FunctionValue)new DerivativeFunction().Apply(new Value[] { f });

        var series = UseCase.Sample(derivative, -3, 3, new PlotSampleRequest(SampleCount: 25));

        series.Points.Should().OnlyContain(p => p.Y.HasValue);
        foreach (var point in series.Points)
        {
            var analytic = 3 * point.X * point.X; // d/dx x^3 = 3x^2
            point.Y!.Value.Should().BeApproximately(analytic, 1e-3);
        }
    }
}
