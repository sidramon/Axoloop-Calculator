namespace Domain.Calculator.Algorithms;

/// <summary>
/// Numerical building blocks absent from <see cref="Math"/>, needed by the continuous
/// distributions (normal via <see cref="Erf"/>, others via <see cref="Gamma"/>/
/// <see cref="LogGamma"/>). Pure algorithms, no <c>Value</c>/<c>IFunction</c> awareness —
/// the erf/gamma/etc. <c>IFunction</c>s are thin adapters over these.
/// </summary>
public static class SpecialFunctions
{
    // Abramowitz & Stegun 7.1.26 — max absolute error ~1.5e-7, well inside the 1e-7 this
    // task asks for. A rational-approximation shortcut, not a series expansion: cheap,
    // and the normal distribution below never needs more precision than this anyway.
    private const double ErfA1 = 0.254829592;
    private const double ErfA2 = -0.284496736;
    private const double ErfA3 = 1.421413741;
    private const double ErfA4 = -1.453152027;
    private const double ErfA5 = 1.061405429;
    private const double ErfP = 0.3275911;

    public static double Erf(double x)
    {
        var sign = x < 0 ? -1.0 : 1.0;
        x = Math.Abs(x);

        var t = 1.0 / (1.0 + ErfP * x);
        var poly = ((((ErfA5 * t + ErfA4) * t + ErfA3) * t + ErfA2) * t + ErfA1) * t;
        var y = 1.0 - poly * Math.Exp(-x * x);

        return sign * y;
    }

    public static double Erfc(double x) => 1.0 - Erf(x);

    // Lanczos approximation, g=7, 9 terms — accurate to ~1e-15 for Re(x) > 0.5. Below
    // 0.5, Euler's reflection formula (gamma(x)*gamma(1-x) = pi/sin(pi*x)) reduces the
    // problem to the accurate region instead of extrapolating the approximation there.
    private static readonly double[] LanczosCoefficients =
    {
        0.99999999999980993,
        676.5203681218851,
        -1259.1392167224028,
        771.32342877765313,
        -176.61502916214059,
        12.507343278686905,
        -0.13857109526572012,
        9.9843695780195716e-6,
        1.5056327351493116e-7,
    };

    private const double LanczosG = 7.0;

    public static double Gamma(double x)
    {
        if (x == Math.Floor(x) && x <= 0)
            throw new InvalidOperationException("gamma is undefined at non-positive integers.");

        if (x < 0.5)
            return Math.PI / (Math.Sin(Math.PI * x) * Gamma(1 - x));

        x -= 1;
        var a = LanczosCoefficients[0];
        var t = x + LanczosG + 0.5;
        for (var i = 1; i < LanczosCoefficients.Length; i++)
            a += LanczosCoefficients[i] / (x + i);

        return Math.Sqrt(2 * Math.PI) * Math.Pow(t, x + 0.5) * Math.Exp(-t) * a;
    }

    /// <summary>
    /// log(gamma(x)), computed directly in log space rather than as Log(Gamma(x)) — Gamma
    /// overflows a double past x ≈ 171, while LogGamma stays finite far beyond that.
    /// Reflection still applies below 0.5, via log(pi/sin(pi*x)) - LogGamma(1-x).
    /// </summary>
    public static double LogGamma(double x)
    {
        if (x == Math.Floor(x) && x <= 0)
            throw new InvalidOperationException("gamma is undefined at non-positive integers.");

        if (x < 0.5)
            return Math.Log(Math.PI / Math.Abs(Math.Sin(Math.PI * x))) - LogGamma(1 - x);

        x -= 1;
        var a = LanczosCoefficients[0];
        var t = x + LanczosG + 0.5;
        for (var i = 1; i < LanczosCoefficients.Length; i++)
            a += LanczosCoefficients[i] / (x + i);

        return 0.5 * Math.Log(2 * Math.PI) + (x + 0.5) * Math.Log(t) - t + Math.Log(a);
    }
}
