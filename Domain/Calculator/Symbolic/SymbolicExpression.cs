namespace Domain.Calculator.Symbolic;

/// <summary>
/// A symbolic algebra tree, distinct from <see cref="Domain.Calculator.Ast.IExpression"/>:
/// the AST carries evaluation behaviour (a <c>BinaryExpression</c> holds an injected
/// <c>IOperator</c>), so a rewrite rule can only recognize "this is a sum" by checking
/// operator identity. Here, structure IS the meaning — pattern-matching on
/// <see cref="Sum"/>/<see cref="Product"/>/<see cref="Power"/> is exactly what a rule
/// needs, and canonical form (e.g. <c>a + b + c</c> as one flat n-ary sum, never nested
/// binary additions) is what makes collecting like terms tractable at all.
///
/// There is no subtraction or division node: <c>a - b</c> is <c>Sum(a, Product(-1, b))</c>
/// and <c>a / b</c> is <c>Product(a, Power(b, -1))</c>. Halving the node vocabulary at the
/// door is what keeps every later rule (folding, collecting, sorting) a two-case match
/// instead of four.
/// </summary>
public abstract record SymbolicExpression;

public sealed record Number(Rational Value) : SymbolicExpression;

public sealed record Symbol(string Name) : SymbolicExpression;

/// <summary>
/// n-ary sum. Equality/hashing are overridden because records compare list-typed
/// properties by reference, not element-wise — without this, two structurally identical
/// sums built from separate lists would compare unequal, breaking every equality-based
/// test the canonicalizer relies on.
/// </summary>
public sealed record Sum(IReadOnlyList<SymbolicExpression> Terms) : SymbolicExpression
{
    public bool Equals(Sum? other) => other is not null && Terms.SequenceEqual(other.Terms);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var term in Terms) hash.Add(term);
        return hash.ToHashCode();
    }
}

/// <summary>n-ary product. See <see cref="Sum"/> for why equality/hashing are overridden.</summary>
public sealed record Product(IReadOnlyList<SymbolicExpression> Factors) : SymbolicExpression
{
    public bool Equals(Product? other) => other is not null && Factors.SequenceEqual(other.Factors);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var factor in Factors) hash.Add(factor);
        return hash.ToHashCode();
    }
}

public sealed record Power(SymbolicExpression Base, SymbolicExpression Exponent) : SymbolicExpression;

/// <summary>See <see cref="Sum"/> for why equality/hashing are overridden.</summary>
public sealed record FunctionCall(string Name, IReadOnlyList<SymbolicExpression> Arguments) : SymbolicExpression
{
    public bool Equals(FunctionCall? other) =>
        other is not null && Name == other.Name && Arguments.SequenceEqual(other.Arguments);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Name);
        foreach (var argument in Arguments) hash.Add(argument);
        return hash.ToHashCode();
    }
}
