namespace Domain.Calculator.Values;

using System.Numerics;
using Domain.Calculator.Algorithms;

/// <summary>
/// A complex number. <see cref="System.Numerics.Complex"/> is used internally for the
/// arithmetic (<see cref="ToComplex"/>/<see cref="Of(Complex)"/>) but never exposed
/// directly as a <see cref="Value"/> — wrapping it here keeps formatting, equality and
/// reduction under this codebase's control instead of BCL defaults.
///
/// Construction always goes through <see cref="Of(double, double)"/>, never the raw
/// positional constructor: it reduces to a plain <see cref="NumberValue"/> when the
/// imaginary part is within <see cref="ReductionTolerance"/> of zero, so <c>_i * _i</c>
/// produces a real <c>-1</c> rather than a complex <c>-1 + 0i</c> that would silently
/// turn ordinary real arithmetic into complex results. The tolerance reuses
/// <see cref="Algorithms.MatrixArrays.Tolerance"/>, the same numerical-noise threshold
/// already used throughout the domain's algorithms.
/// </summary>
public sealed record ComplexValue(double Real, double Imaginary) : Value
{
    public const double ReductionTolerance = MatrixArrays.Tolerance;

    public static Value Of(double real, double imaginary) =>
        Math.Abs(imaginary) < ReductionTolerance
            ? new NumberValue(real)
            : new ComplexValue(real, imaginary);

    public static Value Of(Complex c) => Of(c.Real, c.Imaginary);

    public Complex ToComplex() => new(Real, Imaginary);

    public double Modulus() => ToComplex().Magnitude;

    public double Argument() => ToComplex().Phase;
}
