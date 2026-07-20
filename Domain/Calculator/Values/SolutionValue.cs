namespace Domain.Calculator.Values;

/// <summary>
/// The result of <c>solve</c>: named after the unknown it was solved for, since a bare
/// <see cref="NumberValue"/> or <see cref="MatrixValue"/> can't carry that name for display.
/// <see cref="TotalFound"/> may exceed <see cref="Values"/>.Count when the root count was
/// capped for display — the real count is preserved so the caller can be told about it.
/// </summary>
public sealed record SolutionValue(
    string Unknown,
    IReadOnlyList<double> Values,
    int TotalFound) : Value;
