namespace Domain.Calculator.Algorithms;

using Domain.Calculator.Plotting;

public enum ExtremumKind { Minimum, Maximum }

public readonly record struct Extremum(double X, double Y, ExtremumKind Kind);

/// <summary>
/// Finds local extrema in a sampled function by scanning for a sign change in the slope
/// between consecutive valid samples, then refining the candidate by parabolic
/// interpolation over the three points bracketing it. Closed-form: unlike
/// <see cref="RootFinding"/> it never needs to re-evaluate the function.
/// </summary>
public static class ExtremaFinding
{
    /// <summary>
    /// Gaps (null <see cref="PlotPoint.Y"/>, from an evaluation exception or an asymptote
    /// break) block slope comparisons across them, exactly like <see cref="RootFinding"/> —
    /// a discontinuity crosses from one extreme to the other without there being a turning
    /// point. Flat runs (equal consecutive Y values) are grouped and treated as one
    /// candidate at their center. The first and last sample are never candidates: nothing
    /// is known about the function beyond the sampled domain.
    /// </summary>
    public static IReadOnlyList<Extremum> Find(IReadOnlyList<PlotPoint> samples, double tolerance)
    {
        var extrema = new List<Extremum>();
        var n = samples.Count;
        if (n < 3) return extrema;

        // slopeSign[i] describes the segment from samples[i] to samples[i+1]: its sign,
        // or null if either endpoint is a gap.
        var slopeSign = new int?[n - 1];
        for (var i = 0; i < n - 1; i++)
        {
            if (samples[i].Y is { } y0 && samples[i + 1].Y is { } y1)
                slopeSign[i] = Math.Sign(y1 - y0);
        }

        var index = 0;
        while (index < slopeSign.Length)
        {
            if (slopeSign[index] is not { } signBefore || signBefore == 0)
            {
                index++;
                continue;
            }

            // Skip over a flat run (zero slope), so long as it isn't interrupted by a gap.
            var after = index + 1;
            while (after < slopeSign.Length && slopeSign[after] == 0) after++;

            if (after >= slopeSign.Length || slopeSign[after] is not { } signAfter)
            {
                index = after; // ran into a gap or the end of the samples; nothing to compare
                continue;
            }

            if (signAfter != 0 && signAfter != signBefore)
            {
                var candidateIndex = (index + 1 + after) / 2;
                var kind = signBefore > 0 ? ExtremumKind.Maximum : ExtremumKind.Minimum;
                extrema.Add(Refine(samples, index, candidateIndex, after + 1, kind, tolerance));
            }

            index = after;
        }

        return extrema;
    }

    private static Extremum Refine(
        IReadOnlyList<PlotPoint> samples, int beforeIndex, int centerIndex, int afterIndex,
        ExtremumKind kind, double tolerance)
    {
        var center = samples[centerIndex];

        // Parabolic interpolation assumes three uniformly spaced points, which only holds
        // when the candidate isn't a multi-point plateau; otherwise the plateau's own
        // center is already the best available estimate (its Y is exact — flat by
        // definition — only its precise X within the flat run is unknowable).
        if (afterIndex != beforeIndex + 2)
            return new Extremum(center.X, center.Y!.Value, kind);

        var before = samples[beforeIndex];
        var after = samples[afterIndex];

        if (before.Y is not { } y0 || center.Y is not { } y1 || after.Y is not { } y2)
            return new Extremum(center.X, center.Y!.Value, kind);

        var h = center.X - before.X;
        var a = ((y0 + y2) / 2 - y1) / (h * h);
        var b = (y2 - y0) / (2 * h);

        if (Math.Abs(a) < tolerance)
            return new Extremum(center.X, y1, kind);

        var tStar = -b / (2 * a);
        if (tStar < -h || tStar > h)
            return new Extremum(center.X, y1, kind);

        var vertexX = center.X + tStar;
        var vertexY = a * tStar * tStar + b * tStar + y1;
        return new Extremum(vertexX, vertexY, kind);
    }
}
