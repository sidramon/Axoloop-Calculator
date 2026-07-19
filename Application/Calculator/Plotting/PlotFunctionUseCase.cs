namespace Application.Calculator.Plotting;

using Domain.Calculator;
using Domain.Calculator.Algorithms;
using Domain.Calculator.Plotting;
using Domain.Calculator.Values;

/// <summary>
/// Samples a user-defined, single-parameter function (resolved through
/// <see cref="FunctionContext"/>) into a <see cref="PlotSeries"/> ready to render.
/// Never lets an evaluation failure escape: a single bad point becomes a gap, not a
/// failed plot. Only structural problems (unknown function, wrong arity, an
/// inconsistent domain) throw.
/// </summary>
public sealed class PlotFunctionUseCase
{
    private readonly FunctionContext _functions;
    private readonly Evaluator _evaluator;
    private readonly VariableContext _globalContext;

    public PlotFunctionUseCase(FunctionContext functions, Evaluator evaluator, VariableContext globalContext)
    {
        _functions = functions;
        _evaluator = evaluator;
        _globalContext = globalContext;
    }

    public PlotSeries Sample(string functionName, double xMin, double xMax, PlotSampleRequest request)
    {
        if (!(xMax > xMin))
            throw new InvalidOperationException($"Invalid domain: xMin ({xMin}) must be less than xMax ({xMax}).");

        var function = ResolveFunction(functionName);

        var (sampleXMin, sampleXMax) = request.Oversample
            ? Widen(xMin, xMax, request.OversampleFactor)
            : (xMin, xMax);

        var sampleCount = Math.Max(2, request.SampleCount);
        var rawPoints = EvaluateRawPoints(function, sampleXMin, sampleXMax, sampleCount);

        var (yMin, yMax) = request.YBounds
            ?? ComputeYWindow(rawPoints, request.LowerPercentile, request.UpperPercentile, request.YMarginFraction);

        var points = ApplyAsymptoteBreaks(rawPoints, yMin, yMax, request.MagnitudeMultiplier, request.JumpMultiplier);

        var zeros = RootFinding.FindSignChanges(points, x => EvaluateAt(function, x), request.ZeroTolerance);
        var extrema = ExtremaFinding.Find(points, request.ExtremumTolerance);

        return new PlotSeries(functionName, points, sampleXMin, sampleXMax, yMin, yMax, zeros, extrema);
    }

    private UserFunction ResolveFunction(string name)
    {
        if (!_functions.TryGet(name, out var function))
            throw new InvalidOperationException(
                $"Undefined function '{name}'. Define it first, e.g. '{name}(x) := ...'.");

        if (function.Parameters.Count != 1)
            throw new InvalidOperationException(
                $"'{name}' must take exactly one parameter to be plotted (it takes {function.Parameters.Count}).");

        return function;
    }

    private double? EvaluateAt(UserFunction function, double x)
    {
        var scope = _globalContext.CreateChild();
        scope.Bind(function.Parameters[0], new NumberValue(x));

        Value result;
        try
        {
            result = _evaluator.Evaluate(function.Body, scope);
        }
        catch
        {
            return null;
        }

        if (result is not NumberValue number)
            return null;

        return double.IsNaN(number.Number) || double.IsInfinity(number.Number)
            ? null
            : number.Number;
    }

    private List<PlotPoint> EvaluateRawPoints(UserFunction function, double xMin, double xMax, int sampleCount)
    {
        var points = new List<PlotPoint>(sampleCount);
        var step = (xMax - xMin) / (sampleCount - 1);

        for (var i = 0; i < sampleCount; i++)
        {
            var x = xMin + step * i;
            points.Add(new PlotPoint(x, EvaluateAt(function, x)));
        }

        return points;
    }

    private static (double YMin, double YMax) ComputeYWindow(
        IReadOnlyList<PlotPoint> points, double lowerPercentile, double upperPercentile, double marginFraction)
    {
        var finiteValues = points
            .Where(p => p.Y.HasValue)
            .Select(p => p.Y!.Value)
            .OrderBy(y => y)
            .ToList();

        if (finiteValues.Count == 0)
            return (-1, 1);

        var yMin = Percentile(finiteValues, lowerPercentile);
        var yMax = Percentile(finiteValues, upperPercentile);

        if (!(yMax > yMin))
        {
            var mid = (yMin + yMax) / 2;
            return (mid - 1, mid + 1);
        }

        var margin = (yMax - yMin) * marginFraction;
        return (yMin - margin, yMax + margin);
    }

    private static double Percentile(IReadOnlyList<double> sortedValues, double percentile)
    {
        if (sortedValues.Count == 1) return sortedValues[0];

        var rank = percentile / 100.0 * (sortedValues.Count - 1);
        var lowerIndex = (int)Math.Floor(rank);
        var upperIndex = (int)Math.Ceiling(rank);
        if (lowerIndex == upperIndex) return sortedValues[lowerIndex];

        var fraction = rank - lowerIndex;
        return sortedValues[lowerIndex] + (sortedValues[upperIndex] - sortedValues[lowerIndex]) * fraction;
    }

    private static IReadOnlyList<PlotPoint> ApplyAsymptoteBreaks(
        IReadOnlyList<PlotPoint> points, double yMin, double yMax, double magnitudeMultiplier, double jumpMultiplier)
    {
        var amplitude = Math.Max(yMax - yMin, 1e-12);
        var center = (yMin + yMax) / 2;
        var magnitudeThreshold = amplitude * magnitudeMultiplier;
        var jumpThreshold = amplitude * jumpMultiplier;

        var result = new PlotPoint[points.Count];
        double? previousY = null;

        for (var i = 0; i < points.Count; i++)
        {
            var point = points[i];

            if (point.Y is not { } y)
            {
                result[i] = point;
                previousY = null;
                continue;
            }

            var tooFarFromCenter = Math.Abs(y - center) > magnitudeThreshold;
            var suddenJump = previousY is { } prev && Math.Abs(y - prev) > jumpThreshold;

            if (tooFarFromCenter || suddenJump)
            {
                result[i] = point with { Y = null };
                previousY = null;
            }
            else
            {
                result[i] = point;
                previousY = y;
            }
        }

        return result;
    }

    private static (double Min, double Max) Widen(double xMin, double xMax, double factor)
    {
        var center = (xMin + xMax) / 2;
        var halfRange = (xMax - xMin) / 2 * factor;
        return (center - halfRange, center + halfRange);
    }
}
