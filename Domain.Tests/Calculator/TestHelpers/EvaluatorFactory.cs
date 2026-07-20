namespace Domain.Tests.Calculator.TestHelpers;

using Domain.Calculator;
using Domain.Calculator.Operations.Functions;
using Domain.Calculator.Operations.Functions.Matrix;
using Domain.Calculator.Operations.Functions.Matrix.Eigen;
using Domain.Calculator.Operations.Functions.Scalar;
using Domain.Calculator.Operations.Functions.Scalar.Trigonometric;
using Domain.Calculator.Operations.SpecialForms;

// Kept intentionally in sync with the `functions` array wired up in Presentation/Program.cs —
// the documentation completeness guard only ever inspects the builtins listed here.

public static class EvaluatorFactory
{
    public static Evaluator CreateDefault() => CreateDefault(new FunctionContext());

    public static Evaluator CreateDefault(FunctionContext functionContext) =>
        new(Builtins(), SpecialForms(), functionContext);

    public static IReadOnlyList<IFunction> Builtins() => new IFunction[]
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
        new DerivFunction(),
        new DerivativeFunction(),
        new NthDerivativeFunction(),
        new IntegralFunction(),
        new AntiderivativeFunction(),
        new AntiderivativeFunction(hasExplicitBasePoint: true),
        new PlotFunction(),
    };

    public static IReadOnlyList<ISpecialForm> SpecialForms() => new ISpecialForm[]
    {
        new IfForm(),
        new SolveForm(),
        new SolveForm(hasExplicitDomain: true),
    };
}
