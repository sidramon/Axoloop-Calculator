namespace Domain.Calculator.Symbolic;

/// <summary>
/// Rewrites a <see cref="SymbolicExpression"/> to a deterministic canonical form: flattened
/// n-ary sums/products, folded constants, removed identities, collected like terms, merged
/// powers of a common base, and a total order over sibling terms/factors so that two
/// expressions equal "on paper" (e.g. <c>x + y</c> and <c>y + x</c>) produce the identical
/// tree.
///
/// This is deliberately an aggressive, deterministic, ALWAYS-TERMINATING pass rather than an
/// open rewrite-rule engine: an engine is a project of its own, and its termination problem
/// is real. What this does NOT do — polynomial expansion (<c>(a+b)*c</c> stays a product of
/// a sum), trigonometric/logarithmic identities, factoring — is out of scope, and each would
/// need its own rule layered on top later.
///
/// Two conventions worth being explicit about: <c>0^0 = 1</c> (see <see cref="Rational.Pow"/>),
/// and <c>x^0 = 1</c> for a symbolic base x, even though that is technically wrong at x = 0 —
/// this is the standard CAS convention (a polynomial's constant term can be written
/// <c>x^0</c> uniformly), not an oversight.
/// </summary>
public static class Canonicalizer
{
    private const int MaxIterations = 100;

    public static SymbolicExpression Canonicalize(SymbolicExpression expr)
    {
        var current = expr;
        for (var i = 0; i < MaxIterations; i++)
        {
            var next = CanonicalizeOnce(current);
            if (next.Equals(current)) return next;
            current = next;
        }

        throw new InvalidOperationException(
            "Canonicalization did not reach a fixed point within the iteration limit.");
    }

    /// <summary>
    /// One full bottom-up rewrite pass: every child is already canonical by the time its
    /// parent's rules run. In practice this alone is a fixed point (a canonical tree
    /// canonicalizes to itself), so <see cref="Canonicalize"/>'s loop above almost always
    /// stops after the second call; the loop exists as a safety net, not because this pass
    /// is expected to need many rounds.
    /// </summary>
    private static SymbolicExpression CanonicalizeOnce(SymbolicExpression expr) => expr switch
    {
        Number or Symbol => expr,
        Sum sum => CanonicalizeSum(sum),
        Product product => CanonicalizeProduct(product),
        Power power => CanonicalizePower(power),
        FunctionCall call => new FunctionCall(call.Name, call.Arguments.Select(CanonicalizeOnce).ToList()),
        _ => throw new InvalidOperationException($"Unknown symbolic expression: {expr.GetType().Name}"),
    };

    private static SymbolicExpression CanonicalizeSum(Sum sum)
    {
        // Flatten: a canonicalized child that is itself a Sum contributes its terms
        // directly, so "a + (b + c)" and "(a + b) + c" both end up as one 3-term Sum.
        var flatTerms = new List<SymbolicExpression>();
        foreach (var rawTerm in sum.Terms)
        {
            var term = CanonicalizeOnce(rawTerm);
            if (term is Sum nested) flatTerms.AddRange(nested.Terms);
            else flatTerms.Add(term);
        }

        // Fold every Number term into a single constant.
        var constant = Rational.Zero;
        var symbolicTerms = new List<SymbolicExpression>();
        foreach (var term in flatTerms)
        {
            if (term is Number n) constant += n.Value;
            else symbolicTerms.Add(term);
        }

        // Collect like terms: x + x -> 2*x, 3*x + 5*x -> 8*x. Two terms are "like" when
        // they share the same base once their own numeric coefficient is stripped off.
        var groups = new List<(SymbolicExpression Base, Rational Coefficient)>();
        foreach (var term in symbolicTerms)
        {
            var (coefficient, baseExpr) = SplitCoefficient(term);
            var index = groups.FindIndex(g => g.Base.Equals(baseExpr));
            if (index >= 0)
                groups[index] = (baseExpr, groups[index].Coefficient + coefficient);
            else
                groups.Add((baseExpr, coefficient));
        }

        var resultTerms = new List<SymbolicExpression>();
        if (!constant.IsZero) resultTerms.Add(new Number(constant));
        foreach (var (baseExpr, coefficient) in groups)
        {
            if (coefficient.IsZero) continue;
            resultTerms.Add(coefficient.Equals(Rational.One) ? baseExpr : BuildProduct(new List<SymbolicExpression> { new Number(coefficient), baseExpr }));
        }

        resultTerms.Sort(CompareExpressions);

        return resultTerms.Count switch
        {
            0 => new Number(Rational.Zero),
            1 => resultTerms[0],
            _ => new Sum(resultTerms),
        };
    }

    private static SymbolicExpression CanonicalizeProduct(Product product)
    {
        // Flatten, same rationale as Sum.
        var flatFactors = new List<SymbolicExpression>();
        foreach (var rawFactor in product.Factors)
        {
            var factor = CanonicalizeOnce(rawFactor);
            if (factor is Product nested) flatFactors.AddRange(nested.Factors);
            else flatFactors.Add(factor);
        }

        // Fold constants; *0 absorbs the whole product regardless of what else is in it.
        var constant = Rational.One;
        var symbolicFactors = new List<SymbolicExpression>();
        foreach (var factor in flatFactors)
        {
            if (factor is Number n) constant *= n.Value;
            else symbolicFactors.Add(factor);
        }
        if (constant.IsZero) return new Number(Rational.Zero);

        // Merge powers of a common base: x*x -> x^2, x^2*x^3 -> x^5. Only ever combines
        // when both exponents are plain Numbers — a symbolic exponent (x^a * x^b) is left
        // as two separate factors rather than guessed at as x^(a+b).
        var groups = new List<(SymbolicExpression Base, SymbolicExpression Exponent)>();
        foreach (var factor in symbolicFactors)
        {
            var (baseExpr, exponent) = SplitPower(factor);
            var index = groups.FindIndex(g => g.Base.Equals(baseExpr) && g.Exponent is Number && exponent is Number);
            if (index >= 0)
            {
                var combined = ((Number)groups[index].Exponent).Value + ((Number)exponent).Value;
                groups[index] = (baseExpr, new Number(combined));
            }
            else
            {
                groups.Add((baseExpr, exponent));
            }
        }

        var finalFactors = new List<SymbolicExpression>();
        if (!constant.Equals(Rational.One)) finalFactors.Add(new Number(constant));
        foreach (var (baseExpr, exponent) in groups)
        {
            var powered = BuildPower(baseExpr, exponent);
            if (powered is Number n && n.Value.Equals(Rational.One)) continue; // x^0 collapsed to 1: acts as *1
            finalFactors.Add(powered);
        }

        finalFactors.Sort(CompareExpressions);

        return finalFactors.Count switch
        {
            0 => new Number(Rational.One),
            1 => finalFactors[0],
            _ => new Product(finalFactors),
        };
    }

    private static SymbolicExpression CanonicalizePower(Power power)
    {
        var baseExpr = CanonicalizeOnce(power.Base);
        var exponent = CanonicalizeOnce(power.Exponent);

        // (x^a)^b -> x^(a*b): exponents multiply, not add, when raising a power to a power.
        // Same "numeric exponents only" restriction as Product's merging.
        if (baseExpr is Power inner && inner.Exponent is Number innerExponent && exponent is Number outerExponent)
            return BuildPower(inner.Base, new Number(innerExponent.Value * outerExponent.Value));

        // (f1*f2*...)^n -> f1^n * f2^n * ...: distributing over a product's factors is what
        // unifies "a/(b*c)" and "a/b/c" onto the same tree (n = -1 in that case). Only for
        // an INTEGER n. A fractional exponent is unsound in general once factors can be
        // negative — (-4*-9)^(1/2) = 6, but (-4)^(1/2)*(-9)^(1/2) isn't defined over the
        // reals — and a symbolic exponent's sign/integrality is simply unknown, so neither
        // is distributed. Re-canonicalizing the freshly built product lets it flatten into
        // its parent and merge with any like powers already there (e.g. (x*x)^2 -> x^2*x^2
        // -> x^4), rather than stopping at the intermediate distributed-but-unmerged form.
        if (baseExpr is Product product && exponent is Number distributiveExponent && distributiveExponent.Value.IsInteger)
        {
            var distributed = new Product(product.Factors.Select(f => (SymbolicExpression)new Power(f, exponent)).ToList());
            return CanonicalizeOnce(distributed);
        }

        return BuildPower(baseExpr, exponent);
    }

    /// <summary>
    /// Applies x^0 = 1 (including 0^0 = 1, see class doc), x^1 = x, and exact integer
    /// exponentiation of a numeric base; anything else is left as an unevaluated
    /// <see cref="Power"/> node. Zero raised to a negative power throws — an exact division
    /// by zero, not a value to approximate as infinity.
    /// </summary>
    private static SymbolicExpression BuildPower(SymbolicExpression baseExpr, SymbolicExpression exponent)
    {
        if (exponent is Number e)
        {
            if (e.Value.IsZero) return new Number(Rational.One);
            if (e.Value.Equals(Rational.One)) return baseExpr;

            if (baseExpr is Number b)
            {
                if (b.Value.IsZero)
                {
                    if (e.Value.IsNegative)
                        throw new DivideByZeroException("Cannot raise zero to a negative power.");
                    return new Number(Rational.Zero);
                }

                if (e.Value.IsInteger)
                    return new Number(b.Value.Pow((int)e.Value.Numerator));

                // Fractional exponent on a numeric base (e.g. 4^(1/2)): the result is
                // generally irrational, so there is no exact Rational to fold it to —
                // left as an unevaluated Power.
            }
        }

        return new Power(baseExpr, exponent);
    }

    private static (Rational Coefficient, SymbolicExpression Base) SplitCoefficient(SymbolicExpression term) =>
        term is Product product && product.Factors.Count > 0 && product.Factors[0] is Number n
            ? (n.Value, BuildProduct(product.Factors.Skip(1).ToList()))
            : (Rational.One, term);

    private static (SymbolicExpression Base, SymbolicExpression Exponent) SplitPower(SymbolicExpression factor) =>
        factor is Power power ? (power.Base, power.Exponent) : (factor, new Number(Rational.One));

    private static SymbolicExpression BuildProduct(IReadOnlyList<SymbolicExpression> factors) => factors.Count switch
    {
        0 => new Number(Rational.One),
        1 => factors[0],
        _ => new Product(factors),
    };

    /// <summary>
    /// Total order over sibling terms/factors: numbers, then symbols (alphabetically),
    /// then products, then powers, then function calls. This is what makes "x + y" and
    /// "y + x" produce the identical tree — sorting any permutation of the same terms with
    /// a consistent order always yields the same sequence.
    /// </summary>
    private static int CompareExpressions(SymbolicExpression a, SymbolicExpression b)
    {
        var tierComparison = Tier(a).CompareTo(Tier(b));
        if (tierComparison != 0) return tierComparison;

        return (a, b) switch
        {
            (Number na, Number nb) => na.Value.CompareTo(nb.Value),
            (Symbol sa, Symbol sb) => string.CompareOrdinal(sa.Name, sb.Name),
            (Product pa, Product pb) => CompareLists(pa.Factors, pb.Factors),
            (Power pa, Power pb) => CompareExpressions(pa.Base, pb.Base) switch
            {
                0 => CompareExpressions(pa.Exponent, pb.Exponent),
                var baseComparison => baseComparison,
            },
            (FunctionCall ca, FunctionCall cb) => string.CompareOrdinal(ca.Name, cb.Name) switch
            {
                0 => CompareLists(ca.Arguments, cb.Arguments),
                var nameComparison => nameComparison,
            },
            _ => 0,
        };
    }

    private static int CompareLists(IReadOnlyList<SymbolicExpression> a, IReadOnlyList<SymbolicExpression> b)
    {
        var count = Math.Min(a.Count, b.Count);
        for (var i = 0; i < count; i++)
        {
            var comparison = CompareExpressions(a[i], b[i]);
            if (comparison != 0) return comparison;
        }
        return a.Count.CompareTo(b.Count);
    }

    private static int Tier(SymbolicExpression expr) => expr switch
    {
        Number => 0,
        Symbol => 1,
        Product => 2,
        Power => 3,
        FunctionCall => 4,
        _ => 5,
    };
}
