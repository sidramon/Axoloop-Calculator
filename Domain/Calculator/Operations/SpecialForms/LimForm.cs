namespace Domain.Calculator.Operations.SpecialForms;

using Domain.Calculator.Algorithms;
using Domain.Calculator.Ast;
using Domain.Calculator.Operations.Functions;
using Domain.Calculator.Values;

/// <summary>
/// Numeric limits. A special form for the same reason as <see cref="SolveForm"/> and
/// <see cref="DiffForm"/>: lim(sin(x)/x, x, 0) contains x, defined nowhere — an ordinary
/// call would evaluate the argument first and throw "undefined variable".
/// </summary>
public sealed class LimForm : ISpecialForm
{
    // Passed straight through to LimitEvaluation as its `tolerance` knob (see there for
    // how it's used: as the base for both the divergence/agreement checks and the
    // (deliberately much looser) Richardson-stability check).
    private const double Tolerance = 1e-6;

    private readonly bool _hasExplicitDirection;

    public LimForm(bool hasExplicitDirection = false) => _hasExplicitDirection = hasExplicitDirection;

    public string Name => "lim";
    public int Arity => _hasExplicitDirection ? 4 : 3;
    public FunctionCategory Category => FunctionCategory.Arithmetic;
    public string Signature => _hasExplicitDirection ? "lim(expr, x, target, direction)" : "lim(expr, x, target)";

    public string Description =>
        "Numerically estimates the limit of expr as x approaches target. This is " +
        "NUMERIC, not symbolic: unlike diff, there is no exact-form result and no " +
        "symbolic L'Hopital's rule involved — expr is sampled at a shrinking sequence " +
        "of points near target and the limit is extrapolated (Richardson extrapolation) " +
        "from those samples, the same spirit as ndiff numerically estimating a " +
        "derivative rather than differentiating symbolically. x must be a plain " +
        "identifier naming the limit variable (not a protected constant); it shadows " +
        "any variable of the same name only for the duration of this call, so a global " +
        "of the same name is unaffected afterward, and other globals remain visible " +
        "throughout (lim(a*x, x, 3) sees the current value of a). target may be a " +
        "number, _inf, or -_inf. A point where expr is undefined (division by zero, " +
        "log of a negative number, any evaluation exception, or a non-numeric result) " +
        "is skipped rather than treated as an error — that's the ordinary case near a " +
        "removable singularity like sin(x)/x at x=0 itself; if every sampled point on " +
        "one side is undefined, that side simply has no limit. The result reports one " +
        "of four outcomes: convergence to a value, divergence to +infinity or " +
        "-infinity, the left and right sides converging to genuinely different values " +
        "(the two-sided limit then doesn't exist even though each side individually is " +
        "well-behaved), or no coherent limit at all — oscillation, e.g. sin(1/x) at " +
        "x=0, where the samples neither settle nor diverge. " +
        (_hasExplicitDirection
            ? "direction picks one side only: negative approaches from below " +
              "(x -> target from the left), positive from above (x -> target from the " +
              "right); zero is rejected as ambiguous."
            : "Pass a fourth argument, lim(expr, x, target, direction), to request only " +
              "one side instead of both.");

    public IReadOnlyList<string> Examples => _hasExplicitDirection
        ? new[]
        {
            "lim(1/x, x, 0, 1) → x → 0⁺ : +∞",
            "lim(1/x, x, 0, -1) → x → 0⁻ : -∞",
        }
        : new[]
        {
            "lim(sin(x)/x, x, 0) → x → 0 : 1",
            "lim((x^2 - 4)/(x - 2), x, 2) → x → 2 : 4",
            "lim(1/x, x, 0) → x → 0⁻ : -∞, x → 0⁺ : +∞ (two-sided limit does not exist)",
            "lim(1/x, x, _inf) → x → ∞ : 0",
        };

    public Value Apply(IReadOnlyList<IExpression> arguments, VariableContext context, Evaluator evaluator)
    {
        var variable = RequireVariableName(arguments[1], context);
        var target = RequireNumber(evaluator.Evaluate(arguments[2], context), "target");

        int? direction = null;
        if (_hasExplicitDirection)
        {
            var directionValue = RequireNumber(evaluator.Evaluate(arguments[3], context), "direction");
            if (directionValue == 0)
                throw new InvalidOperationException(
                    "'lim' direction must be negative (approach from below) or positive " +
                    "(from above), not zero.");
            direction = directionValue < 0 ? -1 : 1;
        }

        // A fresh child scope, bound and rebound at every sampled point — the parent
        // `context` itself is never touched, so the limit variable can't leak into it
        // regardless of what happens below; this mirrors SolveForm/DiffForm exactly.
        var scope = context.CreateChild();
        var expression = arguments[0];

        double? Residual(double t)
        {
            scope.Bind(variable, new NumberValue(t));
            try
            {
                var value = evaluator.Evaluate(expression, scope);
                return value is NumberValue number && !double.IsNaN(number.Number) ? number.Number : null;
            }
            catch
            {
                return null;
            }
        }

        var result = LimitEvaluation.Evaluate(Residual, target, Tolerance);

        if (direction is { } d)
        {
            var sideValue = d < 0 ? result.LeftValue : result.RightValue;
            return new LimitValue(variable, target, d, ReduceToSide(sideValue), sideValue, null, null);
        }

        return new LimitValue(variable, target, null, result.Kind, result.Value, result.LeftValue, result.RightValue);
    }

    private static LimitKind ReduceToSide(double? value) => value switch
    {
        null => LimitKind.NoLimit,
        double.PositiveInfinity => LimitKind.DivergesToPositiveInfinity,
        double.NegativeInfinity => LimitKind.DivergesToNegativeInfinity,
        _ => LimitKind.Converges
    };

    private static string RequireVariableName(IExpression argument, VariableContext context)
    {
        if (argument is not IdentifierExpression identifier)
            throw new InvalidOperationException(
                "'lim' second argument must name the variable, e.g. lim(sin(x)/x, x, 0).");

        if (context.IsProtected(identifier.Name))
            throw new InvalidOperationException(
                $"'{identifier.Name}' is a protected constant and cannot be used as the limit variable.");

        return identifier.Name;
    }

    private static double RequireNumber(Value value, string argumentName)
    {
        if (value is not NumberValue number)
            throw new InvalidOperationException($"'lim' {argumentName} must be a number.");
        return number.Number;
    }
}
