namespace Domain.Tests.Calculator.TestHelpers;

using Domain.Calculator;
using Domain.Calculator.Operations.Functions;
using Domain.Calculator.Operations.Functions.Matrix;
using Domain.Calculator.Operations.Functions.Matrix.Eigen;
using Domain.Calculator.Operations.Functions.Scalar;
using Domain.Calculator.Operations.Functions.Scalar.Trigonometric;
using Domain.Calculator.Operations.SpecialForms;

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
    };

    public static IReadOnlyList<ISpecialForm> SpecialForms() => new ISpecialForm[]
    {
        new IfForm(),
    };
}
