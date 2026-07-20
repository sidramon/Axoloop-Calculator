namespace Domain.Calculator.Symbolic;

using System.Text;

/// <summary>
/// Renders a <see cref="SymbolicExpression"/> as a string that reparses (via the same
/// grammar the evaluator uses) to an equivalent tree — that round trip is the actual proof
/// that the parenthesization below is correct, not just plausible-looking.
///
/// Parentheses are driven by a precedence level per node (Sum &lt; Product &lt; Power &lt;
/// atom), the same ordering the real parser uses for +,-,*,/,^. A child is wrapped only
/// when its own level is lower than what the parent slot requires — e.g. a Sum inside a
/// Product's factor list always gets parens, but a Product inside a Sum's term list never
/// needs them.
///
/// Two cases don't fit the generic "compare precedence levels" rule and are special-cased
/// deliberately:
/// - A non-integer <see cref="Rational"/> prints with an embedded '/' (e.g. "2/3") even
///   though it is a single atomic <see cref="Number"/> node; that '/' is only safe in a
///   context that already tolerates Product-level text, so such a Number reports
///   Product-level precedence instead of atom-level.
/// - A negative integer used as the BASE of a <see cref="Power"/> (e.g. (-2)^x) must be
///   parenthesized even though negative integers are otherwise printed unparenthesized
///   anywhere else: "-2^x" would reparse as -(2^x), because the parser's unary minus binds
///   an entire power-tower operand, not just the atom that follows it.
/// </summary>
public static class SymbolicPrinter
{
    private const int SumLevel = 4;
    private const int ProductLevel = 5;
    private const int PowerLevel = 6;
    private const int AtomLevel = 100;

    private static readonly Rational NegativeOne = new(-1);

    public static string Print(SymbolicExpression expression) => PrintChild(expression, 0);

    private static string PrintChild(SymbolicExpression expression, int minPrecedence)
    {
        var (text, precedence) = PrintNode(expression);
        return precedence < minPrecedence ? $"({text})" : text;
    }

    private static (string Text, int Precedence) PrintNode(SymbolicExpression expression) => expression switch
    {
        Number number => PrintNumber(number.Value),
        Symbol symbol => (symbol.Name, AtomLevel),
        Sum sum => (PrintSum(sum), SumLevel),
        Product product => PrintProduct(product),
        Power power => PrintPower(power),
        FunctionCall call => (PrintFunctionCall(call), AtomLevel),
        _ => throw new InvalidOperationException($"Unknown symbolic expression: {expression.GetType().Name}"),
    };

    private static (string, int) PrintNumber(Rational value) =>
        value.IsInteger
            ? (value.Numerator.ToString(), AtomLevel)
            : ($"{value.Numerator}/{value.Denominator}", ProductLevel);

    private static string PrintSum(Sum sum)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < sum.Terms.Count; i++)
        {
            var (negative, magnitude) = ExtractSign(sum.Terms[i]);
            var text = PrintChild(magnitude, negative ? ProductLevel : SumLevel);

            if (i == 0)
                sb.Append(negative ? "-" : "");
            else
                sb.Append(negative ? " - " : " + ");
            sb.Append(text);
        }
        return sb.ToString();
    }

    private static (string, int) PrintProduct(Product product)
    {
        var (negative, magnitude) = ExtractSign(product);
        if (negative)
            return ($"-{PrintChild(magnitude, ProductLevel)}", ProductLevel);

        var numerator = new List<SymbolicExpression>();
        var denominator = new List<SymbolicExpression>();
        foreach (var factor in product.Factors)
        {
            if (TryGetReciprocalBase(factor, out var reciprocalBase))
                denominator.Add(reciprocalBase);
            else
                numerator.Add(factor);
        }

        var numeratorText = numerator.Count == 0
            ? "1"
            : string.Join("*", numerator.Select(f => PrintChild(f, ProductLevel)));

        if (denominator.Count == 0)
            return (numeratorText, ProductLevel);

        // The denominator sits to the right of '/', which is left-associative at the same
        // precedence as '*': "a/b*c" is (a/b)*c, not a/(b*c). So unlike a numerator factor
        // (safe at ProductLevel, since chained '*' is associative), the denominator as a
        // whole needs parens whenever it is anything other than a single atom or a Power —
        // hence ProductLevel + 1, not ProductLevel, whether it came from one reciprocal
        // factor whose base is itself compound (a/(b*c)) or from several separate
        // reciprocal factors (a/b/c collected as one denominator).
        var denominatorExpression = BuildProduct(denominator);
        var denominatorText = PrintChild(denominatorExpression, ProductLevel + 1);

        return ($"{numeratorText}/{denominatorText}", ProductLevel);
    }

    private static (string, int) PrintPower(Power power)
    {
        // Power(x, -1) -> 1/x, and more generally Power(x, negative) -> 1/x^|exponent| —
        // the same "reciprocal is more readable" policy AstConverter applies, kept
        // consistent between the two.
        if (power.Exponent is Number e && e.Value.IsNegative)
        {
            var positiveBase = e.Value.Equals(NegativeOne) ? power.Base : new Power(power.Base, new Number(-e.Value));
            // Same reasoning as PrintProduct's denominator: this sits to the right of '/',
            // so it needs ProductLevel + 1, not ProductLevel, or "1/(a*b)" would print as
            // the wrong-precedence "1/a*b" (= (1/a)*b).
            return ($"1/{PrintChild(positiveBase, ProductLevel + 1)}", ProductLevel);
        }

        // A negative integer base needs explicit parens even though it would otherwise be
        // atom-level — see the class doc's second bullet.
        var baseText = power.Base is Number { Value.IsNegative: true }
            ? $"({PrintChild(power.Base, 0)})"
            : PrintChild(power.Base, PowerLevel + 1);

        var exponentText = PrintChild(power.Exponent, PowerLevel);

        return ($"{baseText}^{exponentText}", PowerLevel);
    }

    private static string PrintFunctionCall(FunctionCall call) =>
        $"{call.Name}({string.Join(", ", call.Arguments.Select(a => PrintChild(a, 0)))})";

    private static bool TryGetReciprocalBase(SymbolicExpression factor, out SymbolicExpression reciprocalBase)
    {
        if (factor is Power power && power.Exponent is Number exponent && exponent.Value.IsNegative)
        {
            reciprocalBase = exponent.Value.Equals(NegativeOne) ? power.Base : new Power(power.Base, new Number(-exponent.Value));
            return true;
        }

        reciprocalBase = null!;
        return false;
    }

    /// <summary>
    /// Whether expression is "negative" for printing purposes, and what to print instead
    /// once the sign is pulled out as a leading '-': a bare negative Number negates to its
    /// absolute value, and a Product whose leading factor is a negative Number drops (or
    /// flips) that leading coefficient — Product(-1, x) reports its magnitude as plain x,
    /// which is what turns "Product(-1, x)" into "-x" rather than "-1*x".
    /// </summary>
    private static (bool IsNegative, SymbolicExpression Magnitude) ExtractSign(SymbolicExpression expression)
    {
        if (expression is Number n && n.Value.IsNegative)
            return (true, new Number(-n.Value));

        if (expression is Product product && product.Factors.Count > 0 && product.Factors[0] is Number leading && leading.Value.IsNegative)
        {
            var negatedLeading = -leading.Value;
            var rest = product.Factors.Skip(1).ToList();
            var magnitude = negatedLeading.Equals(Rational.One)
                ? BuildProduct(rest)
                : BuildProduct(Prepend(new Number(negatedLeading), rest));
            return (true, magnitude);
        }

        return (false, expression);
    }

    private static SymbolicExpression BuildProduct(IReadOnlyList<SymbolicExpression> factors) => factors.Count switch
    {
        0 => new Number(Rational.One),
        1 => factors[0],
        _ => new Product(factors),
    };

    private static List<SymbolicExpression> Prepend(SymbolicExpression head, IReadOnlyList<SymbolicExpression> rest)
    {
        var list = new List<SymbolicExpression>(rest.Count + 1) { head };
        list.AddRange(rest);
        return list;
    }
}
