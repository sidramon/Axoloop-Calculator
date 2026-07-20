namespace Domain.Calculator.Operations.SpecialForms;

using Domain.Calculator.Algorithms;
using Domain.Calculator.Ast;
using Domain.Calculator.Operations.Functions;
using Domain.Calculator.Plotting;
using Domain.Calculator.Values;

public sealed class SolveForm : ISpecialForm
{
    private const double DefaultXMin = -100;
    private const double DefaultXMax = 100;
    private const int SampleCount = 4001;
    private const double Tolerance = 1e-9;
    private const int MaxReturnedRoots = 10;

    private readonly bool _hasExplicitDomain;

    public SolveForm(bool hasExplicitDomain = false) => _hasExplicitDomain = hasExplicitDomain;

    public string Name => "solve";
    public int Arity => _hasExplicitDomain ? 4 : 2;
    public FunctionCategory Category => FunctionCategory.Arithmetic;

    public string Signature => "solve(equation, unknown) or solve(equation, unknown, xMin, xMax)";

    public string Description =>
        "Numerically solves an equation for its unknown, written exactly as you think it — " +
        "e.g. solve(3*x + 2 = 5, x) — rather than a matrix or a pre-built function. Roots " +
        "are approximated, not symbolic: solve(3*x = 5, x) returns 1.6667, not 5/3. equation " +
        "may be an equality (split at the top-level '=') or a bare expression, treated as " +
        "expression = 0; any other top-level comparison ('<', '>=', ...) or a chained " +
        "equality ('a = b = c') throws, since solve resolves equalities only. unknown must " +
        "be a plain identifier naming the variable to solve for — it temporarily shadows any " +
        "variable of the same name while solving, without affecting it afterward, and cannot " +
        "be a protected constant. By default the domain [-100, 100] is scanned for sign " +
        "changes; pass explicit bounds as solve(equation, unknown, xMin, xMax) to search " +
        "elsewhere, or to isolate one root among several. Roots are returned sorted ascending, " +
        "named after the unknown (e.g. 'x = 2.5'); a periodic equation can have many roots " +
        "over a wide domain, so at most 10 are shown — solving still finds the true count and " +
        "says so when it exceeds that, suggesting a narrower explicit domain. No root in the " +
        "domain throws, naming the domain searched. Distinct from linsolve/linsolvegen, which " +
        "take matrices A and b for a matrix equation A*x = b.";

    public IReadOnlyList<string> Examples => new[]
    {
        "solve(3*x = 5, x) → 1.6667",
        "solve(x^2 = 4, x) → [-2, 2]",
        "solve(sin(x) = 0.5, x) → the first 10 of the many roots within [-100, 100], plus a " +
            "note of how many were actually found",
        "solve(exp(x) = 1000, x, 0, 20) → a root within the given domain",
    };

    public Value Apply(IReadOnlyList<IExpression> arguments, VariableContext context, Evaluator evaluator)
    {
        var (left, right) = ParseEquation(arguments[0]);
        var unknown = RequireUnknownName(arguments[1], context);

        var xMin = DefaultXMin;
        var xMax = DefaultXMax;
        if (_hasExplicitDomain)
        {
            xMin = RequireNumber(evaluator.Evaluate(arguments[2], context), "xMin");
            xMax = RequireNumber(evaluator.Evaluate(arguments[3], context), "xMax");
            if (!(xMax > xMin))
                throw new InvalidOperationException(
                    $"Invalid domain: xMin ({xMin}) must be less than xMax ({xMax}).");
        }

        var scope = context.CreateChild();
        var roots = FindRoots(left, right, scope, unknown, evaluator, xMin, xMax);

        if (roots.Count == 0)
        {
            var hint = _hasExplicitDomain
                ? "."
                : "; try solve(equation, unknown, xMin, xMax) with an explicit domain.";
            throw new InvalidOperationException($"no solution found in [{xMin}, {xMax}]{hint}");
        }

        var totalFound = roots.Count;
        var returned = totalFound > MaxReturnedRoots ? roots.Take(MaxReturnedRoots).ToList() : roots;

        return new SolutionValue(unknown, returned, totalFound);
    }

    private static (IExpression Left, IExpression Right) ParseEquation(IExpression equation)
    {
        if (equation is BinaryExpression { Operator: EqualsOperator } binary)
        {
            if (IsComparison(binary.Left) || IsComparison(binary.Right))
                throw new InvalidOperationException(
                    "'solve' expects a single equality, not a chained comparison such as 'a = b = c'.");

            return (binary.Left, binary.Right);
        }

        if (IsComparison(equation))
            throw new InvalidOperationException("'solve' expects an equality ('='), not another comparison.");

        return (equation, new NumberExpression(new NumberValue(0)));
    }

    private static bool IsComparison(IExpression expression) => expression is BinaryExpression
    {
        Operator: EqualsOperator or LessOperator or GreaterOperator or LessOrEqualOperator or GreaterOrEqualOperator,
    };

    private static string RequireUnknownName(IExpression argument, VariableContext context)
    {
        if (argument is not IdentifierExpression identifier)
            throw new InvalidOperationException(
                "'solve' second argument must name the unknown, e.g. solve(3*x = 5, x).");

        if (context.IsProtected(identifier.Name))
            throw new InvalidOperationException(
                $"'{identifier.Name}' is a protected constant and cannot be used as the unknown.");

        return identifier.Name;
    }

    private static double RequireNumber(Value value, string argumentName)
    {
        if (value is not NumberValue number)
            throw new InvalidOperationException($"'solve' {argumentName} must be a number.");
        return number.Number;
    }

    private static IReadOnlyList<double> FindRoots(
        IExpression left, IExpression right, VariableContext scope, string unknown,
        Evaluator evaluator, double xMin, double xMax)
    {
        double? Residual(double x)
        {
            scope.Bind(unknown, new NumberValue(x));

            Value leftValue, rightValue;
            try
            {
                leftValue = evaluator.Evaluate(left, scope);
                rightValue = evaluator.Evaluate(right, scope);
            }
            catch
            {
                return null;
            }

            if (leftValue is not NumberValue leftNumber || rightValue is not NumberValue rightNumber)
                return null;

            var result = leftNumber.Number - rightNumber.Number;
            return double.IsNaN(result) || double.IsInfinity(result) ? null : result;
        }

        var step = (xMax - xMin) / (SampleCount - 1);
        var samples = new List<PlotPoint>(SampleCount);
        for (var i = 0; i < SampleCount; i++)
        {
            var x = xMin + step * i;
            samples.Add(new PlotPoint(x, Residual(x)));
        }

        var roots = RootFinding.FindSignChanges(samples, Residual, Tolerance);
        return Deduplicate(roots, Tolerance);
    }

    private static IReadOnlyList<double> Deduplicate(IReadOnlyList<double> roots, double tolerance)
    {
        var sorted = roots.OrderBy(r => r).ToList();
        var result = new List<double>();
        foreach (var root in sorted)
        {
            if (result.Count == 0 || root - result[^1] > tolerance)
                result.Add(root);
        }
        return result;
    }
}
