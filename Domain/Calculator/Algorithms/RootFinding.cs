namespace Domain.Calculator.Algorithms;

using Domain.Calculator.Plotting;

/// <summary>
/// Finds zero-crossings in a sampled function. Deliberately decoupled from the
/// evaluator: bisection re-evaluates the function through a plain
/// Func&lt;double, double?&gt; delegate supplied by the caller.
/// </summary>
public static class RootFinding
{
    private const int MaxBisectionIterations = 100;

    /// <summary>
    /// Scans consecutive valid sample pairs for a sign change and refines each one by
    /// bisection. Gaps (null <see cref="PlotPoint.Y"/>, whether from an evaluation
    /// exception or an asymptote break) are skipped rather than bridged — an asymptote
    /// jump crosses zero without there being a root there.
    /// </summary>
    public static IReadOnlyList<double> FindSignChanges(
        IReadOnlyList<PlotPoint> samples,
        Func<double, double?> evaluate,
        double tolerance)
    {
        var roots = new List<double>();

        for (var i = 1; i < samples.Count; i++)
        {
            var previous = samples[i - 1];
            var current = samples[i];

            if (previous.Y is not { } y1 || current.Y is not { } y2)
                continue;

            if (y1 == 0)
            {
                // Already recorded as "current" in the previous iteration, unless it's the
                // very first sample — otherwise a sample landing exactly on zero would be
                // reported twice, once approached from each side.
                if (i == 1) roots.Add(previous.X);
                continue;
            }

            if (Math.Sign(y1) == Math.Sign(y2))
                continue;

            if (y2 == 0)
            {
                roots.Add(current.X);
                continue;
            }

            var root = Bisect(previous.X, current.X, evaluate, tolerance);
            if (root.HasValue)
                roots.Add(root.Value);
        }

        return roots;
    }

    private static double? Bisect(double lower, double upper, Func<double, double?> evaluate, double tolerance)
    {
        if (evaluate(lower) is not { } yLower)
            return null;
        if (evaluate(upper) is not { } yUpperInitial)
            return null;

        // A genuine root is where |f| shrinks toward zero as the bracket narrows. An
        // asymptote the sampling grid failed to flag as a gap instead grows without bound
        // as the bracket narrows — bisection still "converges" there (the sign keeps
        // flipping), so it must be told apart from a real root after the fact.
        var initialScale = Math.Max(Math.Abs(yLower), Math.Abs(yUpperInitial));

        for (var i = 0; i < MaxBisectionIterations && upper - lower > tolerance; i++)
        {
            var mid = (lower + upper) / 2;

            if (evaluate(mid) is not { } yMid)
                return null;

            if (Math.Sign(yMid) == Math.Sign(yLower))
            {
                lower = mid;
                yLower = yMid;
            }
            else
            {
                upper = mid;
            }
        }

        var root = (lower + upper) / 2;
        if (evaluate(root) is not { } yFinal || Math.Abs(yFinal) > initialScale)
            return null;

        return root;
    }
}
