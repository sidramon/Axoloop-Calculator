namespace Domain.Calculator.Values;

/// <summary>
/// A read-only sequence of values with no single shared numeric representation — e.g.
/// eigvals results that mix real and complex eigenvalues. Distinct from
/// <see cref="MatrixValue"/>, whose backing store is <c>double[,]</c> and can only ever
/// hold reals.
/// </summary>
public sealed record ValueListValue(IReadOnlyList<Value> Values) : Value;
