namespace Domain.Calculator.Values;

using Domain.Calculator.Algorithms;

/// <summary>
/// The result of <c>lim(...)</c>: named after the variable and target it was evaluated
/// at, since a bare <see cref="NumberValue"/> can't carry that context, and because a
/// limit that doesn't converge to a single finite value (diverges, has different
/// one-sided values, or has no limit at all) has no number to fall back to in the first
/// place. Always a <see cref="LimitValue"/>, even on ordinary convergence — never
/// collapsed to a plain <see cref="NumberValue"/> — for the same reason
/// <see cref="SolutionValue"/> is: the variable/target context is worth keeping around
/// for display, and every outcome (converge, diverge, disagree, oscillate) then goes
/// through one consistent type instead of the result's type depending on which of four
/// outcomes happened to occur.
///
/// <see cref="Direction"/> is null for a two-sided <c>lim(expr, x, target)</c> call, or
/// -1/+1 for a one-sided <c>lim(expr, x, target, direction)</c> call. In the one-sided
/// case, <see cref="Kind"/>/<see cref="Value"/> describe only the requested side, and
/// <see cref="LeftValue"/>/<see cref="RightValue"/> are always null — there is no "other
/// side" to report.
/// </summary>
public sealed record LimitValue(
    string Variable,
    double Target,
    int? Direction,
    LimitKind Kind,
    double? Value,
    double? LeftValue,
    double? RightValue) : Value;
