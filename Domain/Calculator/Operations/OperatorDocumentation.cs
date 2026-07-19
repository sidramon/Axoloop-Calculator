namespace Domain.Calculator.Operations;

public enum OperatorKind
{
    Assignment,
    Binary,
    PrefixUnary,
    PostfixUnary,
    Logical
}

public sealed record OperatorDocumentationEntry(
    string Symbol,
    OperatorKind Kind,
    int? Precedence,
    Associativity? Associativity,
    string Description,
    string Example);

/// <summary>
/// Static documentation for every operator and logical keyword in the language.
/// Single source for the console help and the web page — the actual precedence table
/// lives in <see cref="Parsing.Parser"/>; this list only describes it.
/// </summary>
public static class OperatorDocumentation
{
    public static IReadOnlyList<OperatorDocumentationEntry> All { get; } = new[]
    {
        new OperatorDocumentationEntry(
            ":=",
            OperatorKind.Assignment,
            null,
            null,
            "Assigns a variable, or defines a function if the name is followed by " +
            "parentheses (f(x) := ...). Not part of the precedence table: recognized only " +
            "at the start of a statement, never in the middle of an expression — " +
            "a := b := 5 fails rather than chaining assignments.",
            "x := 2+3 → 5"),

        new OperatorDocumentationEntry(
            "or",
            OperatorKind.Logical,
            1,
            Operations.Associativity.Left,
            "Logical disjunction, short-circuiting: if the left operand is True, the right " +
            "operand is NOT evaluated. Requires two booleans. The lowest precedence in the " +
            "whole language.",
            "1 > 2 or 2 > 1 → True"),

        new OperatorDocumentationEntry(
            "and",
            OperatorKind.Logical,
            2,
            Operations.Associativity.Left,
            "Logical conjunction, short-circuiting: if the left operand is False, the right " +
            "operand is NOT evaluated. Requires two booleans. Binds tighter than or, looser " +
            "than the comparisons.",
            "1 < 2 and 3 > 2 → True"),

        new OperatorDocumentationEntry(
            "not",
            OperatorKind.Logical,
            3,
            null,
            "Logical negation, prefix. Binds tighter than and/or but looser than the " +
            "comparisons: \"not a < b\" reads as \"not (a < b)\" — the comparison is " +
            "evaluated before the negation. Requires a boolean.",
            "not 1 > 2 → True"),

        new OperatorDocumentationEntry(
            "=",
            OperatorKind.Binary,
            3,
            Operations.Associativity.Left,
            "Equality (test), not to be confused with := (assignment). Requires two " +
            "numbers, returns a boolean.",
            "1+2 = 3 → True"),

        new OperatorDocumentationEntry(
            "<",
            OperatorKind.Binary,
            3,
            Operations.Associativity.Left,
            "Strictly less than. Requires two numbers.",
            "3 < 5 → True"),

        new OperatorDocumentationEntry(
            ">",
            OperatorKind.Binary,
            3,
            Operations.Associativity.Left,
            "Strictly greater than. Requires two numbers.",
            "5 > 5 → False"),

        new OperatorDocumentationEntry(
            "<=",
            OperatorKind.Binary,
            3,
            Operations.Associativity.Left,
            "Less than or equal. The tokenizer recognizes two-character operators before " +
            "single-character ones: \"a <= b\" is never read as \"a < (= b)\".",
            "4 <= 4 → True"),

        new OperatorDocumentationEntry(
            ">=",
            OperatorKind.Binary,
            3,
            Operations.Associativity.Left,
            "Greater than or equal.",
            "4 >= 5 → False"),

        new OperatorDocumentationEntry(
            "+",
            OperatorKind.Binary,
            4,
            Operations.Associativity.Left,
            "Addition: number+number, or element-wise matrix addition if both operands are " +
            "matrices of the same dimension.",
            "2+3*4 → 14 (* binds tighter than +)"),

        new OperatorDocumentationEntry(
            "-",
            OperatorKind.Binary,
            4,
            Operations.Associativity.Left,
            "Subtraction (binary). Distinct from the prefix negation -x, which has a much " +
            "higher precedence.",
            "5 - 3 → 2"),

        new OperatorDocumentationEntry(
            "*",
            OperatorKind.Binary,
            5,
            Operations.Associativity.Left,
            "Multiplication: number*number, number*matrix (scaling, commutative in that " +
            "specific case) or matrix*matrix (matrix product, not commutative in general).",
            "3*-2 → -6"),

        new OperatorDocumentationEntry(
            "/",
            OperatorKind.Binary,
            5,
            Operations.Associativity.Left,
            "Division: number/number or matrix/number (scaling). Dividing by zero throws a " +
            "DivideByZeroException instead of returning Infinity.",
            "10/4 → 2.5"),

        new OperatorDocumentationEntry(
            "%",
            OperatorKind.Binary,
            5,
            Operations.Associativity.Left,
            "Modulo (remainder of division), numbers only — no matrix variant.",
            "7 % 3 → 1"),

        new OperatorDocumentationEntry(
            "^",
            OperatorKind.Binary,
            6,
            Operations.Associativity.Right,
            "Exponentiation. RIGHT-associative, unlike +, -, * and /: " +
            "2^3^2 = 2^(3^2) = 512, not (2^3)^2 = 64.",
            "2^3^2 → 512"),

        new OperatorDocumentationEntry(
            "-",
            OperatorKind.PrefixUnary,
            6,
            null,
            "Prefix negation (number, or matrix element-wise). Binds only the operand of " +
            "highest precedence (a tower of powers), not the whole expression that " +
            "follows: -2^2 = -(2^2) = -4, but (-2)^2 = 4.",
            "-2^2 → -4"),

        new OperatorDocumentationEntry(
            "!",
            OperatorKind.PostfixUnary,
            null,
            null,
            "Factorial, postfix. Requires a non-negative integer. Binds tighter than any " +
            "binary or prefix operator, including negation: -3! = -(3!) = -6, not (-3)!.",
            "5! → 120"),
    };
}
