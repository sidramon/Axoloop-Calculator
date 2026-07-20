namespace Domain.Calculator.Ast;

/// <summary>
/// A call whose target is an already-produced expression rather than a name — e.g. the
/// second call in <c>deriv(f)(3)</c>. Distinct from <see cref="CallExpression"/>, which
/// still owns name resolution (builtin, special form, user function) for the first call
/// following an identifier.
/// </summary>
public sealed record InvokeExpression(IExpression Target, IReadOnlyList<IExpression> Arguments) : IExpression;
