namespace Domain.Calculator.Algorithms;

public enum LimitKind
{
    Converges,
    DivergesToPositiveInfinity,
    DivergesToNegativeInfinity,
    OneSidedDiffer,
    NoLimit
}

public sealed record LimitResult(
    LimitKind Kind,
    double? Value,
    double? LeftValue,
    double? RightValue);

/// <summary>
/// Numerically estimates lim(g(t), t -> target): samples g at a geometrically shrinking
/// sequence of offsets from target (or, for an infinite target, a geometrically growing
/// sequence of |t|), then extrapolates with Richardson extrapolation rather than
/// trusting the smallest raw sample directly.
///
/// THE central numerical hazard is catastrophic cancellation. For sin(x)/x near x=0,
/// pushing the sampled offset toward machine epsilon eventually samples pure rounding
/// noise in both numerator and denominator — the computed quotient becomes arbitrary
/// well BEFORE reaching the mathematical limit; precision degrades before the descent
/// gets there, not after. This stops descending at a floor (1e-6 by default, see
/// <see cref="EpsilonFloor"/>) rather than continuing toward 1e-16, and estimates the
/// limit from the last few still-well-conditioned samples via Richardson extrapolation
/// instead of reading off the point closest to target.
///
/// <see cref="Func{T,TResult}"/> g returns null for a point where evaluation failed or
/// is undefined (division by zero, an exception, NaN) — that point is skipped and the
/// sequence continues, which is the ordinary case near a removable singularity like
/// sin(x)/x at x=0 itself. If every point on one side is undefined, that side
/// contributes no coherent value at all.
/// </summary>
public static class LimitEvaluation
{
    private const int MaxSteps = 10;
    private const double StepRatio = 0.1;
    private const double FirstStep = 0.1;

    // How aggressively a sample sequence must be growing, in both magnitude and a
    // consistent sign, before it's called divergent rather than "still extrapolating".
    private const double DivergenceMagnitudeFloor = 1e4;
    private const double DivergenceGrowthFactor = 2.0;

    // For an infinite target: geometrically growing |x|, capped well short of the point
    // (~1e15) where 1/x itself would be swallowed by double rounding.
    private const double InfiniteApproachStart = 10.0;
    private const double InfiniteApproachCap = 1e10;

    public static LimitResult Evaluate(Func<double, double?> g, double target, double tolerance)
    {
        if (double.IsPositiveInfinity(target)) return EvaluateAtInfinity(g, positive: true, tolerance);
        if (double.IsNegativeInfinity(target)) return EvaluateAtInfinity(g, positive: false, tolerance);

        var epsilonFloor = EpsilonFloor(target);
        var left = ClassifySide(SampleSide(h => g(target - h), epsilonFloor), tolerance);
        var right = ClassifySide(SampleSide(h => g(target + h), epsilonFloor), tolerance);

        return Combine(left, right, tolerance);
    }

    /// <summary>
    /// The offset below which descent stops, before catastrophic cancellation corrupts
    /// the samples. 1e-6 by default — measured empirically (see LimitEvaluationTests /
    /// the task's own worked example, (x^2-4)/(x-2) at x=2): a naive offset-scaled floor
    /// of ~1e-8 sits right where cancellation noise from evaluating the EXPRESSION
    /// itself (not just target +/- offset, but whatever arithmetic the expression does
    /// with that point — squaring, subtracting, then dividing by a small offset)
    /// dominates the true O(h) signal; 1e-6 stays comfortably clear of that for
    /// everything tested, while still resolving detail far finer than any of the
    /// classification thresholds care about. Only scaled UP for a very large |target|,
    /// where target +/- offset itself needs a wider berth to stay resolvable at all.
    /// </summary>
    private static double EpsilonFloor(double target) => Math.Max(1e-6, Math.Abs(target) * 1e-10);

    private static LimitResult EvaluateAtInfinity(Func<double, double?> g, bool positive, double tolerance)
    {
        var points = new List<(double H, double G)>();
        var magnitude = InfiniteApproachStart;
        while (magnitude <= InfiniteApproachCap)
        {
            var value = g(positive ? magnitude : -magnitude);
            if (value is { } v && double.IsFinite(v))
                points.Add((1.0 / magnitude, v)); // h = 1/magnitude shrinks geometrically, reusing the same Richardson math as the finite-target case
            magnitude /= StepRatio;
        }

        var side = ClassifySide(points, tolerance);

        // Only one direction of approach exists at +-infinity — collapse straight to a
        // result rather than routing through Combine's two-sides logic.
        if (side is null) return new LimitResult(LimitKind.NoLimit, null, null, null);
        if (double.IsPositiveInfinity(side.Value))
            return new LimitResult(LimitKind.DivergesToPositiveInfinity, side, side, side);
        if (double.IsNegativeInfinity(side.Value))
            return new LimitResult(LimitKind.DivergesToNegativeInfinity, side, side, side);
        return new LimitResult(LimitKind.Converges, side, side, side);
    }

    private static List<(double H, double G)> SampleSide(Func<double, double?> gOfOffset, double epsilonFloor)
    {
        var points = new List<(double H, double G)>();
        var h = FirstStep;
        var steps = 0;
        while (h >= epsilonFloor && steps < MaxSteps)
        {
            var value = gOfOffset(h);
            if (value is { } v && double.IsFinite(v))
                points.Add((h, v));
            h *= StepRatio;
            steps++;
        }
        return points;
    }

    /// <summary>
    /// One side's samples, reduced to a signed infinity (divergence), a finite estimate
    /// (convergence), or null (samples neither stabilize nor diverge cleanly — too few
    /// defined points, or oscillation like sin(1/x) near 0).
    /// </summary>
    private static double? ClassifySide(IReadOnlyList<(double H, double G)> points, double tolerance)
    {
        if (points.Count < 2) return null;

        var last = points[^1];
        var secondLast = points[^2];
        var magnitudeGrowing = Math.Abs(last.G) > Math.Abs(secondLast.G) * DivergenceGrowthFactor
            && Math.Abs(last.G) > DivergenceMagnitudeFloor;
        var consistentSign = points.Select(p => Math.Sign(p.G)).Distinct().Count() == 1;
        if (magnitudeGrowing && consistentSign)
            return last.G > 0 ? double.PositiveInfinity : double.NegativeInfinity;

        // Richardson extrapolation on every consecutive pair: assumes g(h) ~= L + C*h
        // near the sampled point, which holds for any function smooth enough to have a
        // genuine limit there. For ratio r = h1/h0, L = (g(h1) - r*g(h0)) / (1 - r).
        var estimates = new List<double>();
        for (var i = 1; i < points.Count; i++)
        {
            var (h0, g0) = points[i - 1];
            var (h1, g1) = points[i];
            var ratio = h1 / h0;
            if (Math.Abs(1 - ratio) < 1e-15) continue;
            var extrapolated = (g1 - ratio * g0) / (1 - ratio);
            if (double.IsFinite(extrapolated)) estimates.Add(extrapolated);
        }
        if (estimates.Count == 0) return null;

        // Stability, not just existence: if consecutive Richardson estimates disagree
        // (an oscillating sequence extrapolates to wildly different "limits" depending
        // on which pair is used), there is no coherent limit to report. Deliberately
        // looser than `tolerance` itself — Richardson estimates from widely-spaced
        // geometric samples carry more residual noise than a single well-behaved sample,
        // and a slowly-converging case like (1+1/x)^x -> e needs the slack.
        var candidate = estimates[^1];
        if (estimates.Count >= 2)
        {
            var previous = estimates[^2];
            var scale = Math.Max(1.0, Math.Abs(candidate));
            if (Math.Abs(candidate - previous) > scale * tolerance * 1000)
                return null;
        }

        return candidate;
    }

    private static LimitResult Combine(double? left, double? right, double tolerance)
    {
        if (left is null || right is null)
            // At least one side has no coherent value at all -- no bilateral conclusion
            // is possible, but whatever the OTHER side did establish is still preserved
            // so a one-sided query in that direction still gets an answer.
            return new LimitResult(LimitKind.NoLimit, null, left, right);

        if (AgreeOrBothSameInfinity(left.Value, right.Value, tolerance))
        {
            if (double.IsPositiveInfinity(left.Value))
                return new LimitResult(LimitKind.DivergesToPositiveInfinity, left, left, right);
            if (double.IsNegativeInfinity(left.Value))
                return new LimitResult(LimitKind.DivergesToNegativeInfinity, left, left, right);
            return new LimitResult(LimitKind.Converges, (left.Value + right.Value) / 2, left, right);
        }

        return new LimitResult(LimitKind.OneSidedDiffer, null, left, right);
    }

    private static bool AgreeOrBothSameInfinity(double a, double b, double tolerance)
    {
        if (double.IsInfinity(a) || double.IsInfinity(b)) return a == b;
        var scale = Math.Max(1.0, Math.Max(Math.Abs(a), Math.Abs(b)));
        return Math.Abs(a - b) < scale * tolerance * 1000;
    }
}
