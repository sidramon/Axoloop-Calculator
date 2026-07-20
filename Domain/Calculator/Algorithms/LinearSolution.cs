namespace Domain.Calculator.Algorithms;

using Domain.Calculator.Values;

public enum SolutionKind
{
    Unique,
    None,
    Infinite,
}

public sealed record LinearSolution(
    SolutionKind Kind,
    MatrixValue? Particular,
    IReadOnlyList<MatrixValue> NullSpaceBasis,
    int Rank,
    int FreeVariables);
