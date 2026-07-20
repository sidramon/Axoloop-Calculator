namespace Domain.Calculator.Values;

using Domain.Calculator.Symbolic;

/// <summary>
/// A symbolic result — e.g. from <c>diff</c> — carried through the evaluator as an
/// ordinary value. Arithmetic on it is rejected in <see cref="Operations.ValueArithmetic"/>
/// the same way it is for <see cref="SolutionValue"/>: composing symbolic expressions
/// (<c>diff(...) + 1</c> building a <c>Sum</c>) is a feature to add deliberately, not a
/// side effect of falling through to numeric arithmetic.
/// </summary>
public sealed record SymbolicValue(SymbolicExpression Expression) : Value;
