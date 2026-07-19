namespace Presentation.App;

public sealed record FormatOptions
{
    public double MinValue { get; init; } = 1e-16;
    public double MaxValue { get; init; } = 1e+16;
    public int Precision   { get; init; } = 10;

    public static FormatOptions Default { get; } = new();
}