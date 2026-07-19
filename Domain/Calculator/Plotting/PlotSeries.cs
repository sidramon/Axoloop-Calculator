namespace Domain.Calculator.Plotting;

using Domain.Calculator.Algorithms;

/// <summary>
/// A sampled function ready to render: the points (with gaps already applied), the
/// domain actually sampled, the Y window retained for display, and the zeros and local
/// extrema found within that domain.
/// </summary>
public sealed record PlotSeries(
    string FunctionName,
    IReadOnlyList<PlotPoint> Points,
    double XMin,
    double XMax,
    double YMin,
    double YMax,
    IReadOnlyList<double> Zeros,
    IReadOnlyList<Extremum> Extrema);
