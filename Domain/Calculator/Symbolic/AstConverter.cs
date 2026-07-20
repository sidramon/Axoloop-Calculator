namespace Domain.Calculator.Symbolic;

using Domain.Calculator.Ast;
using Domain.Calculator.Operations;
using Domain.Calculator.Values;

/// <summary>
/// Bidirectional bridge between the evaluator's <see cref="IExpression"/> AST and the
/// <see cref="SymbolicExpression"/> algebra tree. This is the one place in
/// <c>Symbolic/</c> that necessarily touches <c>Ast/</c> (and, transitively, the
/// <c>Values.NumberValue</c>/<c>Operations.IOperator</c> types that <c>Ast</c> node shapes
/// require) — nothing else in this namespace does.
///
/// Numbers lose exactness going back to AST: <see cref="Values.NumberValue"/> holds a
/// <c>double</c>, so a <see cref="Rational"/> like 1/3 becomes 0.3333... again. That is the
/// unavoidable boundary until a future task lets a symbolic result carry an exact value
/// through <c>Value</c> itself (out of scope here — see task constraint #2).
/// </summary>
public static class AstConverter
{
    private static readonly Rational NegativeOne = new(-1);

    // ---- AST -> Symbolic ----

    public static SymbolicExpression ToSymbolic(IExpression expression) => expression switch
    {
        NumberExpression { Value: NumberValue number } => new Number(Rational.FromDouble(number.Number)),
        IdentifierExpression identifier => new Symbol(identifier.Name),
        BinaryExpression binary => ConvertBinary(binary),
        UnaryExpression unary => ConvertUnary(unary),
        CallExpression call => new FunctionCall(call.Name, call.Arguments.Select(ToSymbolic).ToList()),

        AssignmentExpression => throw NotSymbolic("An assignment has no symbolic value of its own."),
        FunctionDefinitionExpression => throw NotSymbolic("A function definition is a statement, not an expression."),
        MatrixExpression => throw NotSymbolic("Matrices have no symbolic representation."),
        LogicalExpression => throw NotSymbolic("Logical (and/or) expressions are boolean, not symbolic."),
        NotExpression => throw NotSymbolic("Boolean negation ('not') is not a symbolic operation."),
        InvokeExpression => throw NotSymbolic("A chained call has no name to symbolize."),

        _ => throw NotSymbolic($"'{expression.GetType().Name}' has no symbolic representation."),
    };

    private static SymbolicExpression ConvertBinary(BinaryExpression binary)
    {
        var left = ToSymbolic(binary.Left);
        var right = ToSymbolic(binary.Right);

        return binary.Operator.Symbol switch
        {
            "+" => new Sum(new[] { left, right }),
            "-" => new Sum(new[] { left, Negate(right) }),
            "*" => new Product(new[] { left, right }),
            "/" => new Product(new[] { left, new Power(right, new Number(NegativeOne)) }),
            "^" => new Power(left, right),
            _ => throw NotSymbolic(
                $"'{binary.Operator.Symbol}' has no symbolic meaning (comparisons and modulo are boolean/numeric, not algebraic)."),
        };
    }

    private static SymbolicExpression ConvertUnary(UnaryExpression unary) => unary.Operator.Symbol switch
    {
        "-" => Negate(ToSymbolic(unary.Operand)),
        _ => throw NotSymbolic(
            $"'{unary.Operator.Symbol}' has no symbolic representation (e.g. factorial isn't an algebraic operator)."),
    };

    private static SymbolicExpression Negate(SymbolicExpression expr) => new Product(new[] { new Number(NegativeOne), expr });

    private static InvalidOperationException NotSymbolic(string message) => new(message);

    // ---- Symbolic -> AST ----

    /// <summary>
    /// Rebuilds an evaluable <see cref="IExpression"/>, looking up binary operators by
    /// symbol in <paramref name="operators"/> (the same registry the parser/evaluator are
    /// wired with) rather than hardcoding concrete operator classes here.
    /// </summary>
    public static IExpression ToAst(SymbolicExpression expression, IReadOnlyDictionary<string, IOperator> operators) =>
        expression switch
        {
            Number number => NumberAst(number.Value),
            Symbol symbol => new IdentifierExpression(symbol.Name),
            Sum sum => ConvertSum(sum, operators),
            Product product => ConvertProduct(product, operators),
            Power power => ConvertPower(power, operators),
            FunctionCall call => new CallExpression(call.Name, call.Arguments.Select(a => ToAst(a, operators)).ToList()),
            _ => throw new InvalidOperationException($"Unknown symbolic expression: {expression.GetType().Name}"),
        };

    private static IExpression ConvertSum(Sum sum, IReadOnlyDictionary<string, IOperator> operators)
    {
        var add = RequireOperator(operators, "+");

        var result = ToAst(sum.Terms[0], operators);
        for (var i = 1; i < sum.Terms.Count; i++)
            result = new BinaryExpression(result, add, ToAst(sum.Terms[i], operators));
        return result;
    }

    /// <summary>
    /// An n-ary product becomes a left-associative chain of '*', except a factor of the
    /// form base^(negative) becomes a '/' step instead — Power(x, -1) alone is "more
    /// readable" as division, per the task's own framing, and the same policy is applied
    /// consistently here and in <see cref="SymbolicPrinter"/>.
    /// </summary>
    private static IExpression ConvertProduct(Product product, IReadOnlyDictionary<string, IOperator> operators)
    {
        var multiply = RequireOperator(operators, "*");
        var divide = RequireOperator(operators, "/");

        IExpression? result = null;
        foreach (var factor in product.Factors)
        {
            if (TryGetReciprocalBase(factor, out var reciprocalBase))
            {
                var right = ToAst(reciprocalBase, operators);
                result = result is null ? new BinaryExpression(NumberAst(Rational.One), divide, right) : new BinaryExpression(result, divide, right);
            }
            else
            {
                var right = ToAst(factor, operators);
                result = result is null ? right : new BinaryExpression(result, multiply, right);
            }
        }

        return result ?? NumberAst(Rational.One);
    }

    private static IExpression ConvertPower(Power power, IReadOnlyDictionary<string, IOperator> operators)
    {
        if (power.Exponent is Number exponent && exponent.Value.IsNegative)
        {
            var divide = RequireOperator(operators, "/");
            var baseExpression = exponent.Value.Equals(NegativeOne)
                ? power.Base
                : new Power(power.Base, new Number(-exponent.Value));
            return new BinaryExpression(NumberAst(Rational.One), divide, ToAst(baseExpression, operators));
        }

        var caret = RequireOperator(operators, "^");
        return new BinaryExpression(ToAst(power.Base, operators), caret, ToAst(power.Exponent, operators));
    }

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

    private static NumberExpression NumberAst(Rational value) => new(new NumberValue(value.ToDouble()));

    private static IOperator RequireOperator(IReadOnlyDictionary<string, IOperator> operators, string symbol) =>
        operators.TryGetValue(symbol, out var op)
            ? op
            : throw new InvalidOperationException($"No '{symbol}' operator is registered in the supplied operator set.");
}
