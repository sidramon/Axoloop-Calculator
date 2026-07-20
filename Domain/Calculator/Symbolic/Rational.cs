namespace Domain.Calculator.Symbolic;

using System.Numerics;

/// <summary>
/// An exact rational number: numerator and denominator as arbitrary-precision integers,
/// always reduced (GCD-divided) at construction, with the sign carried on the numerator
/// and the denominator always positive and non-zero. Exact arithmetic is the whole point
/// of the symbolic layer — diff(x/3, x) must give 1/3, not the double 0.3333333333333333.
/// </summary>
public readonly struct Rational : IEquatable<Rational>, IComparable<Rational>
{
    public BigInteger Numerator { get; }
    public BigInteger Denominator { get; }

    public static readonly Rational Zero = new(0);
    public static readonly Rational One = new(1);

    public Rational(BigInteger numerator, BigInteger denominator)
    {
        if (denominator == 0)
            throw new DivideByZeroException("A rational's denominator cannot be zero.");

        if (denominator < 0)
        {
            numerator = -numerator;
            denominator = -denominator;
        }

        if (numerator == 0)
        {
            denominator = 1;
        }
        else
        {
            var gcd = BigInteger.GreatestCommonDivisor(BigInteger.Abs(numerator), denominator);
            if (gcd > BigInteger.One)
            {
                numerator /= gcd;
                denominator /= gcd;
            }
        }

        Numerator = numerator;
        Denominator = denominator;
    }

    public Rational(BigInteger integer) : this(integer, BigInteger.One)
    {
    }

    public bool IsZero => Numerator.IsZero;
    public bool IsNegative => Numerator.Sign < 0;
    public bool IsInteger => Denominator.IsOne;

    public static Rational operator +(Rational a, Rational b) =>
        new(a.Numerator * b.Denominator + b.Numerator * a.Denominator, a.Denominator * b.Denominator);

    public static Rational operator -(Rational a, Rational b) =>
        new(a.Numerator * b.Denominator - b.Numerator * a.Denominator, a.Denominator * b.Denominator);

    public static Rational operator -(Rational a) => new(-a.Numerator, a.Denominator);

    public static Rational operator *(Rational a, Rational b) =>
        new(a.Numerator * b.Numerator, a.Denominator * b.Denominator);

    public static Rational operator /(Rational a, Rational b)
    {
        if (b.IsZero)
            throw new DivideByZeroException("Cannot divide by a zero rational.");
        return new Rational(a.Numerator * b.Denominator, a.Denominator * b.Numerator);
    }

    /// <summary>
    /// Integer exponentiation. 0^0 = 1 by convention (the usual combinatorial/CAS
    /// convention — e.g. so that a polynomial's constant term can be written x^0
    /// uniformly even at x = 0); any other zero base with a negative exponent throws,
    /// since that is a genuine division by zero rather than a convention to pick.
    /// </summary>
    public Rational Pow(int exponent)
    {
        if (exponent == 0) return One;

        if (exponent > 0)
            return new Rational(BigInteger.Pow(Numerator, exponent), BigInteger.Pow(Denominator, exponent));

        if (IsZero)
            throw new DivideByZeroException("Cannot raise zero to a negative power.");

        var positiveExponent = -exponent;
        return new Rational(BigInteger.Pow(Denominator, positiveExponent), BigInteger.Pow(Numerator, positiveExponent));
    }

    public Rational Abs() => Numerator.Sign < 0 ? new Rational(-Numerator, Denominator) : this;

    public double ToDouble() => (double)Numerator / (double)Denominator;

    /// <summary>
    /// Approximates a double as a rational via its continued-fraction expansion,
    /// stopping once the denominator would exceed <paramref name="maxDenominator"/>.
    /// This is an approximation, not an exact conversion — a double like 0.1 has no
    /// exact binary representation to begin with, so "exact" here means "the simplest
    /// fraction close enough to the double actually stored", not "the double's precise
    /// value as a fraction" (that would need denominators up to 2^52 for no benefit).
    /// This is the deliberate, documented boundary between the tokenizer's floating-point
    /// world and the symbolic layer's exact one.
    /// </summary>
    public static Rational FromDouble(double value, long maxDenominator = 1_000_000)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
            throw new ArgumentException("Cannot convert NaN or infinity to a rational.", nameof(value));

        if (value == 0) return Zero;

        var negative = value < 0;
        var remainder = Math.Abs(value);

        // Successive convergents h1/k1 (newest) and h0/k0 (previous) of the continued
        // fraction expansion of remainder; stop as soon as the next convergent's
        // denominator would exceed maxDenominator, keeping the last one that didn't.
        long h0 = 0, k0 = 1, h1 = 1, k1 = 0;

        for (var i = 0; i < 64; i++)
        {
            var term = Math.Floor(remainder);
            var termInt = (long)term;

            var h2 = termInt * h1 + h0;
            var k2 = termInt * k1 + k0;
            if (k2 > maxDenominator || k2 <= 0) break;

            h0 = h1; k0 = k1; h1 = h2; k1 = k2;

            var fractional = remainder - term;
            if (fractional < 1e-13) break;
            remainder = 1.0 / fractional;
        }

        if (k1 == 0) return Zero;

        var result = new Rational(h1, k1);
        return negative ? -result : result;
    }

    public static implicit operator Rational(int value) => new(value);
    public static implicit operator Rational(BigInteger value) => new(value);

    public int CompareTo(Rational other) => (Numerator * other.Denominator).CompareTo(other.Numerator * Denominator);

    public static bool operator <(Rational a, Rational b) => a.CompareTo(b) < 0;
    public static bool operator >(Rational a, Rational b) => a.CompareTo(b) > 0;
    public static bool operator <=(Rational a, Rational b) => a.CompareTo(b) <= 0;
    public static bool operator >=(Rational a, Rational b) => a.CompareTo(b) >= 0;
    public static bool operator ==(Rational a, Rational b) => a.Equals(b);
    public static bool operator !=(Rational a, Rational b) => !a.Equals(b);

    public bool Equals(Rational other) => Numerator == other.Numerator && Denominator == other.Denominator;
    public override bool Equals(object? obj) => obj is Rational other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Numerator, Denominator);

    public override string ToString() => Denominator.IsOne ? Numerator.ToString() : $"{Numerator}/{Denominator}";
}
