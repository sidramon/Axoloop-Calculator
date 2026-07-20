namespace Domain.Calculator.Operations.SpecialForms;

using Domain.Calculator.Ast;
using Domain.Calculator.Operations.Functions;
using Domain.Calculator.Symbolic;
using Domain.Calculator.Values;

/// <summary>
/// Exact symbolic differentiation. A special form for the same reason as
/// <see cref="SolveForm"/>: diff(3*x^4, x) contains x, which is defined nowhere — an
/// ordinary call would evaluate the argument first and throw "undefined variable".
/// </summary>
public sealed class DiffForm : ISpecialForm
{
    private readonly FunctionContext _functions;
    private readonly bool _hasExplicitOrder;

    public DiffForm(FunctionContext functions, bool hasExplicitOrder = false)
    {
        _functions = functions;
        _hasExplicitOrder = hasExplicitOrder;
    }

    public string Name => "diff";
    public int Arity => _hasExplicitOrder ? 3 : 2;
    public FunctionCategory Category => FunctionCategory.Arithmetic;
    public string Signature => _hasExplicitOrder ? "diff(expr, x, n)" : "diff(expr, x)";

    public string Description =>
        "Exact, symbolic derivative of expr with respect to x. Unlike ndiff, which " +
        "numerically samples a function at one point, diff takes an expression written in " +
        "terms of x — not evaluated — and returns another expression: diff(3*x^4, x) gives " +
        "12*x^3, not a number. Any other symbol appearing in expr is treated as a constant " +
        "with respect to x regardless of its current value in this session: diff(a*x, x) " +
        "gives a even if a := 3 was assigned earlier, because differentiation works on " +
        "syntactic structure, never on values. x must be a plain identifier naming the " +
        "differentiation variable and cannot be a protected constant. If expr names a " +
        "user-defined function, x must name that function's own parameter exactly " +
        "(diff(f, t) for f(t) := ..., not diff(f, x)) — for a function of several " +
        "parameters this gives the partial derivative with respect to the named one, the " +
        "rest held constant. Throws if expr contains a function with no known derivative " +
        "(e.g. det), or if the result is not purely numeric and x isn't a plain variable. " +
        "diff(expr, x, n) repeats this n times for the nth derivative (n a positive " +
        "integer, capped at 20 to guard against expression blow-up); each pass is " +
        "canonicalized before the next, or intermediate expressions would explode in size.";

    public IReadOnlyList<string> Examples => _hasExplicitOrder
        ? new[]
        {
            "diff(x^3, x, 2) → 6*x",
            "diff(sin(x), x, 3) → -cos(x)",
        }
        : new[]
        {
            "diff(3*x^4, x) → 12*x^3",
            "diff(x*sin(x), x) → sin(x) + x*cos(x)",
            "f(t) := t^2 :: diff(f, t) → 2*t",
        };

    public Value Apply(IReadOnlyList<IExpression> arguments, VariableContext context, Evaluator evaluator)
    {
        var variable = RequireVariableName(arguments[1], context);
        var expression = ResolveExpression(arguments[0], variable);

        var derivative = _hasExplicitOrder
            ? Differentiator.DifferentiateNth(expression, variable, RequirePositiveInteger(evaluator.Evaluate(arguments[2], context)))
            : Differentiator.Differentiate(expression, variable);

        return derivative is Number n ? new NumberValue(n.Value.ToDouble()) : new SymbolicValue(derivative);
    }

    private SymbolicExpression ResolveExpression(IExpression argument, string variable)
    {
        if (argument is IdentifierExpression identifier && _functions.TryGet(identifier.Name, out var function))
        {
            if (!function.Parameters.Contains(variable))
            {
                throw new InvalidOperationException(function.Parameters.Count == 1
                    ? $"'{variable}' does not name '{identifier.Name}''s parameter " +
                      $"'{function.Parameters[0]}'; use diff({identifier.Name}, {function.Parameters[0]})."
                    : $"'{variable}' does not name one of '{identifier.Name}''s parameters " +
                      $"({string.Join(", ", function.Parameters)}).");
            }

            return AstConverter.ToSymbolic(function.Body);
        }

        return AstConverter.ToSymbolic(argument);
    }

    private static string RequireVariableName(IExpression argument, VariableContext context)
    {
        if (argument is not IdentifierExpression identifier)
            throw new InvalidOperationException(
                "diff's second argument must name the variable to differentiate by, e.g. diff(x^2, x).");

        if (context.IsProtected(identifier.Name))
            throw new InvalidOperationException(
                $"'{identifier.Name}' is a protected constant and cannot be used as a differentiation variable.");

        return identifier.Name;
    }

    private static int RequirePositiveInteger(Value value)
    {
        if (value is not NumberValue n)
            throw new InvalidOperationException("diff's order argument must be a number.");
        if (n.Number <= 0 || n.Number % 1 != 0)
            throw new InvalidOperationException("diff's order must be a positive integer.");
        return (int)n.Number;
    }
}
