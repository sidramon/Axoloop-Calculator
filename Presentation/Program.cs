using Presentation.App;
using Application.Calculator;
using Application.Calculator.Documentation;
using Application.Calculator.Plotting;
using Application.Views;
using Domain.Calculator;
using Domain.Calculator.Operations;
using Domain.Calculator.Operations.Functions;
using Domain.Calculator.Operations.Functions.Matrix;
using Domain.Calculator.Operations.Functions.Matrix.Eigen;
using Domain.Calculator.Operations.Functions.Scalar;
using Domain.Calculator.Operations.Functions.Scalar.Trigonometric;
using Domain.Calculator.Operations.SpecialForms;
using Domain.Calculator.Parsing;
using Infrastructure.Documentation;
using Infrastructure.Plotting;
using Infrastructure.Views;
using Presentation.App.IO;
using Presentation.App.Plotting;

var operators = new IOperator[]
{
    new AddOperator(),
    new SubtractOperator(),
    new MultiplyOperator(),
    new DivideOperator(),
    new ModuloOperator(),
    new PowerOperator(),
    new EqualsOperator(),
    new LessOrEqualOperator(),
    new GreaterOrEqualOperator(),
    new LessOperator(),
    new GreaterOperator(),
};

var unaryOperators = new IUnaryOperator[]
{
    new FactorialOperator(),
};

var functions = new IFunction[]
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

var specialForms = new ISpecialForm[]
{
    new IfForm(),
};

var postfixOperators = new IUnaryOperator[]
{
    new FactorialOperator(),
};

var prefixOperators = new IUnaryOperator[]
{
    new NegateOperator(),
};

var functionContext = new FunctionContext();

var parser        = new Parser(operators, postfixOperators, prefixOperators, new Tokenizer());
var evaluator     = new Evaluator(functions, specialForms, functionContext);
var context       = new VariableContext();
context.Seed(Constants.All);

var evaluate         = new EvaluateExpressionUseCase(parser, evaluator, context);
var listVariables    = new ListVariablesUseCase(context);
var listFunctions    = new ListFunctionsUseCase(functionContext);
var functionDocs     = new GetFunctionDocumentationUseCase(functions, specialForms);
var operatorDocs     = new GetOperatorDocumentationUseCase(OperatorDocumentation.All);
var plotFunction     = new PlotFunctionUseCase(functionContext, evaluator, context);
var formatter        = new NumberFormatter(FormatOptions.Default);

IDocumentationRenderer documentationRenderer = new HtmlDocumentationRenderer();
IViewLauncher viewLauncher = new BrowserViewLauncher();

var plotRenderers = new IPlotRenderer[]
{
    new AsciiPlotRenderer(),
    new HtmlPlotRenderer(),
};

var router = new CommandRouter(
    evaluate,
    listVariables,
    listFunctions,
    functionDocs,
    operatorDocs,
    documentationRenderer,
    plotFunction,
    plotRenderers,
    viewLauncher,
    formatter);

var metaCommands = new[]
{
    "/help", "/vars", "/funcs", "/functions", "/operators", "/doc",
    "/plot", "/plotweb", "/zeros", "/extrema", "/clear",
};

var completionHandler = new CalculatorCompletionHandler(
    functions.Select(f => f.Name).ToList(),
    metaCommands,
    listVariables,
    listFunctions);

await new CalculatorRepl(router, completionHandler).RunAsync();
