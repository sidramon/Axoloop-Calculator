namespace Application.Calculator.Plotting;

/// <summary>
/// Rendering options for a <see cref="Domain.Calculator.Plotting.PlotSeries"/>. The
/// visible X window may be narrower than the series' own sampled domain (the web
/// renderer over-samples so the visible window can start as a slice of it); the
/// visible Y window defaults to the series' own retained window when left null.
/// </summary>
public sealed record PlotOptions(
    int Width,
    int Height,
    double VisibleXMin,
    double VisibleXMax,
    double? VisibleYMin = null,
    double? VisibleYMax = null,
    bool ShowZeros = true,
    bool ShowGrid = true);
