namespace Domain.Tests.Calculator.TestHelpers;

using Domain.Calculator;
using Domain.Calculator.Operations.Functions;
using Domain.Calculator.Operations.Functions.Complex;
using Domain.Calculator.Operations.Functions.Matrix;
using Domain.Calculator.Operations.Functions.Matrix.Eigen;
using Domain.Calculator.Operations.Functions.Probability;
using Domain.Calculator.Operations.Functions.Random;
using Domain.Calculator.Operations.Functions.Scalar;
using Domain.Calculator.Operations.Functions.Scalar.Trigonometric;
using Domain.Calculator.Operations.SpecialForms;
using Domain.Calculator.Random;

// Kept intentionally in sync with the `functions` array wired up in Presentation/Program.cs —
// the documentation completeness guard only ever inspects the builtins listed here.

public static class EvaluatorFactory
{
    public static Evaluator CreateDefault() => CreateDefault(new FunctionContext());

    public static Evaluator CreateDefault(FunctionContext functionContext) =>
        CreateDefault(functionContext, new SystemRandomSource());

    /// <summary>
    /// Same as <see cref="CreateDefault(FunctionContext)"/>, but with a caller-supplied
    /// <see cref="IRandomSource"/> — e.g. a <c>ScriptedRandomSource</c> — wired into every
    /// random-number builtin instead of a fresh <see cref="SystemRandomSource"/>, for
    /// tests that need to control or observe what the random functions draw.
    /// </summary>
    public static Evaluator CreateDefault(FunctionContext functionContext, IRandomSource randomSource) =>
        new(Builtins(randomSource), SpecialForms(functionContext), functionContext);

    public static IReadOnlyList<IFunction> Builtins() => Builtins(new SystemRandomSource());

    public static IReadOnlyList<IFunction> Builtins(IRandomSource randomSource) => new IFunction[]
    {
        new SqrtFunction(),
        new NthRootFunction(),
        new SinFunction(),
        new CosFunction(),
        new TanFunction(),
        new AsinFunction(),
        new AcosFunction(),
        new AtanFunction(),
        new Atan2Function(),
        new CscFunction(),
        new SecFunction(),
        new CotFunction(),
        new AcscFunction(),
        new AsecFunction(),
        new AcotFunction(),
        new AbsFunction(),
        new LnFunction(),
        new PowFunction(),
        new LogFunction(),
        new TransposeFunction(),
        new DeterminantFunction(),
        new InverseFunction(),
        new TraceFunction(),
        new RankFunction(),
        new IdentityFunction(),
        new DotProductFunction(),
        new CrossProductFunction(),
        new NormFunction(),
        new ZerosFunction(),
        new OnesFunction(),
        new ReshapeFunction(),
        new EigenvaluesFunction(),
        new EigenvectorsFunction(),
        new IsSymmetricFunction(),
        new LinSolveFunction(),
        new LinSolveGeneralFunction(),
        new RrefFunction(),
        new NullSpaceFunction(),
        new NdiffFunction(),
        new NdiffCallableFunction(),
        new NthNdiffFunction(),
        new IntegralFunction(),
        new AntiderivativeFunction(),
        new AntiderivativeFunction(hasExplicitBasePoint: true),
        new PlotFunction(),
        new RealPartFunction(),
        new ImaginaryPartFunction(),
        new ConjugateFunction(),
        new ArgumentFunction(),
        new MeanFunction(),
        new MedianFunction(),
        new ModeFunction(),
        new VarianceFunction(),
        new VarianceFunction(hasPopulationArgument: true),
        new StdDevFunction(),
        new StdDevFunction(hasPopulationArgument: true),
        new RangeFunction(),
        new SumFunction(),
        new ProductFunction(),
        new MinFunction(),
        new MaxFunction(),
        new ChooseFunction(),
        new PermutationsFunction(),
        new ErfFunction(),
        new ErfcFunction(),
        new GammaFunction(),
        new LogGammaFunction(),
        new NormalPdfFunction(),
        new NormalCdfFunction(),
        new NormalInverseCdfFunction(),
        new BinomialPdfFunction(),
        new BinomialCdfFunction(),
        new PoissonPdfFunction(),
        new PoissonCdfFunction(),
        new RandFunction(randomSource),
        new RandFunction(randomSource, hasCount: true),
        new RandIntFunction(randomSource),
        new RandIntFunction(randomSource, hasCount: true),
        new RandNormFunction(randomSource),
        new RandNormFunction(randomSource, hasCount: true),
        new RandBinFunction(randomSource),
        new RandSampFunction(randomSource),
        new RandSampFunction(randomSource, hasReplacementArgument: true),
        new SeedFunction(randomSource),
    };

    public static IReadOnlyList<ISpecialForm> SpecialForms(FunctionContext? functionContext = null)
    {
        var functions = functionContext ?? new FunctionContext();
        return new ISpecialForm[]
        {
            new IfForm(),
            new SolveForm(),
            new SolveForm(hasExplicitDomain: true),
            new DiffForm(functions),
            new DiffForm(functions, hasExplicitOrder: true),
            new LimForm(),
            new LimForm(hasExplicitDirection: true),
        };
    }
}
