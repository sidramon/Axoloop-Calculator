namespace Domain.Calculator.Plotting;

/// <summary>
/// A single sample of a plotted function. <see cref="Y"/> is null to signal a gap —
/// the curve must break there, never be connected across it.
/// </summary>
public readonly record struct PlotPoint(double X, double? Y);
