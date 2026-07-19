namespace Application.Calculator.Plotting;

/// <summary>
/// Tunable thresholds for <see cref="PlotFunctionUseCase"/>. Every heuristic constant
/// (percentile window, asymptote-break multipliers, zero tolerance) is a parameter here
/// rather than a constant buried in the sampling code.
/// </summary>
public sealed record PlotSampleRequest(
    int SampleCount,
    bool Oversample = false,
    double OversampleFactor = 4.0,
    (double YMin, double YMax)? YBounds = null,
    double LowerPercentile = 2.0,
    double UpperPercentile = 98.0,
    double YMarginFraction = 0.1,
    double MagnitudeMultiplier = 10.0,
    double JumpMultiplier = 10.0,
    double ZeroTolerance = 1e-9,
    double ExtremumTolerance = 1e-9);
