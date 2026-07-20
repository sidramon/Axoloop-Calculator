namespace Domain.Calculator.Symbolic;

/// <summary>
/// Exact, structural differentiation of a <see cref="SymbolicExpression"/> with respect to
/// a named variable. Every other symbol encountered is treated as a constant regardless of
/// what it might be bound to elsewhere — differentiation operates purely on syntactic
/// structure, never on values (see <see cref="Operations.SpecialForms.DiffForm"/>, which
/// deliberately never consults a <c>VariableContext</c> for this reason).
///
/// The raw recursive result is only canonicalized once, at the very end of
/// <see cref="Differentiate"/> — <see cref="DifferentiateNth"/> instead canonicalizes
/// between every single-derivative pass, which is what keeps repeated differentiation from
/// producing an exponentially growing tree.
/// </summary>
public static class Differentiator
{
    private const int MaxOrder = 20;

    public static SymbolicExpression Differentiate(SymbolicExpression expression, string variable) =>
        Canonicalizer.Canonicalize(DifferentiateRaw(expression, variable));

    public static SymbolicExpression DifferentiateNth(SymbolicExpression expression, string variable, int order)
    {
        if (order < 1)
            throw new ArgumentOutOfRangeException(nameof(order), "Derivative order must be a positive integer.");
        if (order > MaxOrder)
            throw new ArgumentOutOfRangeException(
                nameof(order), $"Derivative order is capped at {MaxOrder} to guard against combinatorial blow-up.");

        var current = Canonicalizer.Canonicalize(expression);
        for (var i = 0; i < order; i++)
            current = Differentiate(current, variable);
        return current;
    }

    private static SymbolicExpression DifferentiateRaw(SymbolicExpression expression, string variable) => expression switch
    {
        Number => Num(0),
        Symbol s => Num(s.Name == variable ? 1 : 0),
        Sum sum => new Sum(sum.Terms.Select(t => DifferentiateRaw(t, variable)).ToList()),
        Product product => DifferentiateProduct(product, variable),
        Power power => DifferentiatePower(power, variable),
        FunctionCall call => DifferentiateFunctionCall(call, variable),
        _ => throw new InvalidOperationException($"Unknown symbolic expression: {expression.GetType().Name}"),
    };

    /// <summary>
    /// The n-ary product rule directly, not nested binary products: for f1*f2*...*fk, the
    /// derivative is the sum over i of (f1*...*fi'*...*fk) — each term replaces exactly one
    /// factor with its derivative and leaves the rest untouched.
    /// </summary>
    private static SymbolicExpression DifferentiateProduct(Product product, string variable)
    {
        var factors = product.Factors;
        var terms = new List<SymbolicExpression>(factors.Count);

        for (var i = 0; i < factors.Count; i++)
        {
            var termFactors = new List<SymbolicExpression>(factors.Count);
            for (var j = 0; j < factors.Count; j++)
                termFactors.Add(j == i ? DifferentiateRaw(factors[j], variable) : factors[j]);
            terms.Add(new Product(termFactors));
        }

        return new Sum(terms);
    }

    /// <summary>
    /// Three distinct cases, chosen by which of base/exponent actually depend on the
    /// derivation variable — not by their node type, since a symbolic exponent that happens
    /// not to mention the variable (e.g. diff(x^a, x)) is just as much a "constant exponent"
    /// as a literal Number.
    /// </summary>
    private static SymbolicExpression DifferentiatePower(Power power, string variable)
    {
        var baseHasVariable = ContainsVariable(power.Base, variable);
        var exponentHasVariable = ContainsVariable(power.Exponent, variable);

        if (!baseHasVariable && !exponentHasVariable)
            return Num(0);

        if (!exponentHasVariable)
        {
            // (u^n)' = n * u^(n-1) * u'
            var uPrime = DifferentiateRaw(power.Base, variable);
            var nMinusOne = new Sum(new[] { power.Exponent, Num(-1) });
            return new Product(new SymbolicExpression[] { power.Exponent, new Power(power.Base, nMinusOne), uPrime });
        }

        if (!baseHasVariable)
        {
            // (a^v)' = a^v * ln(a) * v'
            var vPrime = DifferentiateRaw(power.Exponent, variable);
            return new Product(new SymbolicExpression[] { power, Call("ln", power.Base), vPrime });
        }

        // (u^v)' = u^v * (v'*ln(u) + v*u'/u) -- rare, but perfectly differentiable rather
        // than an exception: both base and exponent carry the derivation variable.
        var uPrime2 = DifferentiateRaw(power.Base, variable);
        var vPrime2 = DifferentiateRaw(power.Exponent, variable);
        var inner = new Sum(new SymbolicExpression[]
        {
            new Product(new SymbolicExpression[] { vPrime2, Call("ln", power.Base) }),
            new Product(new SymbolicExpression[] { power.Exponent, uPrime2, Recip(power.Base) }),
        });
        return new Product(new SymbolicExpression[] { power, inner });
    }

    private static bool ContainsVariable(SymbolicExpression expression, string variable) => expression switch
    {
        Number => false,
        Symbol s => s.Name == variable,
        Sum sum => sum.Terms.Any(t => ContainsVariable(t, variable)),
        Product product => product.Factors.Any(f => ContainsVariable(f, variable)),
        Power power => ContainsVariable(power.Base, variable) || ContainsVariable(power.Exponent, variable),
        FunctionCall call => call.Arguments.Any(a => ContainsVariable(a, variable)),
        _ => throw new InvalidOperationException($"Unknown symbolic expression: {expression.GetType().Name}"),
    };

    private static SymbolicExpression DifferentiateFunctionCall(FunctionCall call, string variable)
    {
        // log(x, base) = ln(x) / ln(base): rewriting to the ln identity and differentiating
        // that lets the ordinary product/power/chain rules handle every combination of
        // which argument (if any) carries the derivation variable, instead of a bespoke
        // two-argument chain rule that would have to duplicate the same logic.
        if (call.Name == "log" && call.Arguments.Count == 2)
        {
            var rewritten = new Product(new SymbolicExpression[]
            {
                Call("ln", call.Arguments[0]),
                Recip(Call("ln", call.Arguments[1])),
            });
            return DifferentiateRaw(rewritten, variable);
        }

        if (call.Arguments.Count != 1)
            throw new InvalidOperationException(
                $"No known derivative for '{call.Name}': only single-argument functions have a chain-rule entry.");

        var u = call.Arguments[0];
        var uPrime = DifferentiateRaw(u, variable);
        var outerDerivative = OuterDerivative(call.Name, u);
        return new Product(new SymbolicExpression[] { outerDerivative, uPrime });
    }

    /// <summary>
    /// The chain-rule table: the derivative of name(u) with respect to u itself (the
    /// outer part only — <see cref="DifferentiateFunctionCall"/> multiplies by u'). abs is
    /// included deliberately even though it is not differentiable at u = 0: x/|x| is the
    /// standard symbolic derivative, and the singularity is a fact about the function, not
    /// a reason to leave it out.
    /// </summary>
    private static SymbolicExpression OuterDerivative(string name, SymbolicExpression u) => name switch
    {
        "sin" => Call("cos", u),
        "cos" => Neg(Call("sin", u)),
        "tan" => Sq(Call("sec", u)),
        "asin" => new Power(OneMinusSquare(u), Num(-1, 2)),
        "acos" => Neg(new Power(OneMinusSquare(u), Num(-1, 2))),
        "atan" => Recip(OnePlusSquare(u)),
        "ln" => Recip(u),
        "sqrt" => new Product(new SymbolicExpression[] { Num(1, 2), new Power(u, Num(-1, 2)) }),
        "abs" => new Product(new SymbolicExpression[] { u, Recip(Call("abs", u)) }),
        "csc" => Neg(new Product(new SymbolicExpression[] { Call("csc", u), Call("cot", u) })),
        "sec" => new Product(new SymbolicExpression[] { Call("sec", u), Call("tan", u) }),
        "cot" => Neg(Sq(Call("csc", u))),
        "acsc" => Neg(Recip(new Product(new SymbolicExpression[] { Call("abs", u), new Power(SquareMinusOne(u), Num(1, 2)) }))),
        "asec" => Recip(new Product(new SymbolicExpression[] { Call("abs", u), new Power(SquareMinusOne(u), Num(1, 2)) })),
        "acot" => Neg(Recip(OnePlusSquare(u))),
        _ => throw new InvalidOperationException($"No known derivative for '{name}'."),
    };

    private static SymbolicExpression OneMinusSquare(SymbolicExpression u) => new Sum(new[] { Num(1), Neg(Sq(u)) });
    private static SymbolicExpression OnePlusSquare(SymbolicExpression u) => new Sum(new[] { Num(1), Sq(u) });
    private static SymbolicExpression SquareMinusOne(SymbolicExpression u) => new Sum(new[] { Sq(u), Num(-1) });

    private static Number Num(int value) => new(new Rational(value));
    private static Number Num(int numerator, int denominator) => new(new Rational(numerator, denominator));
    private static SymbolicExpression Neg(SymbolicExpression e) => new Product(new[] { Num(-1), e });
    private static SymbolicExpression Recip(SymbolicExpression e) => new Power(e, Num(-1));
    private static SymbolicExpression Sq(SymbolicExpression e) => new Power(e, Num(2));
    private static FunctionCall Call(string name, SymbolicExpression argument) => new(name, new[] { argument });
}
