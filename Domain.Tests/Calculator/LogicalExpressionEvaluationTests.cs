namespace Domain.Tests.Calculator;

using Domain.Calculator;
using Domain.Calculator.Values;
using Domain.Tests.Calculator.TestHelpers;
using FluentAssertions;
using Value = Domain.Calculator.Values.Value;

public class LogicalExpressionEvaluationTests
{
    private readonly Domain.Calculator.Parsing.Parser _parser = ParserFactory.CreateDefault();
    private readonly Evaluator _evaluator = EvaluatorFactory.CreateDefault();
    private readonly VariableContext _context = new();

    private Value Run(string input) => _evaluator.Evaluate(_parser.Parse(input), _context);

    [Fact]
    public void And_BothOperandsTrue_ReturnsTrue()
    {
        ((BooleanValue)Run("1 < 2 and 3 > 2")).Boolean.Should().BeTrue();
    }

    [Fact]
    public void And_NonBooleanLeftOperand_ThrowsInvalidOperationException()
    {
        var act = () => Run("1 and 2");

        act.Should().Throw<InvalidOperationException>().WithMessage("*and*boolean*");
    }

    [Fact]
    public void And_LeftFalse_ShortCircuits_RightSideNeverEvaluated()
    {
        // If short-circuiting weren't working, evaluating 'undefined_var' would throw
        // instead of the whole expression cleanly returning False.
        var result = (BooleanValue)Run("1 > 2 and undefined_var > 0");

        result.Boolean.Should().BeFalse();
    }

    [Fact]
    public void Or_LeftTrue_ShortCircuits_RightSideNeverEvaluated()
    {
        var result = (BooleanValue)Run("2 > 1 or undefined_var > 0");

        result.Boolean.Should().BeTrue();
    }

    [Fact]
    public void Or_BothOperandsFalse_ReturnsFalse()
    {
        ((BooleanValue)Run("1 > 2 or 3 < 2")).Boolean.Should().BeFalse();
    }

    [Fact]
    public void Not_TrueOperand_ReturnsFalse()
    {
        ((BooleanValue)Run("not 3 > 2")).Boolean.Should().BeFalse();
    }

    [Fact]
    public void Not_FalseOperand_ReturnsTrue()
    {
        ((BooleanValue)Run("not 1 > 2")).Boolean.Should().BeTrue();
    }

    [Fact]
    public void Not_NonBooleanOperand_ThrowsInvalidOperationException()
    {
        var act = () => Run("not 5");

        act.Should().Throw<InvalidOperationException>().WithMessage("*not*boolean*");
    }

    [Fact]
    public void If_TrueCondition_EvaluatesOnlyThenBranch()
    {
        // If the else-branch were evaluated eagerly, the undefined variable would throw.
        var result = (NumberValue)Run("if(1 < 2, 10, undefined_var)");

        result.Number.Should().BeApproximately(10, 1e-10);
    }

    [Fact]
    public void If_FalseCondition_EvaluatesOnlyElseBranch()
    {
        var result = (NumberValue)Run("if(1 > 2, undefined_var, 20)");

        result.Number.Should().BeApproximately(20, 1e-10);
    }

    [Fact]
    public void If_NonBooleanCondition_ThrowsInvalidOperationException()
    {
        var act = () => Run("if(1, 2, 3)");

        act.Should().Throw<InvalidOperationException>().WithMessage("*boolean*");
    }

    [Fact]
    public void If_WrongArity_ThrowsInvalidOperationException()
    {
        var act = () => Run("if(1, 2)");

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ReservedWord_UsedAsAssignmentTarget_ThrowsFormatException()
    {
        var act = () => _parser.Parse("and := 5");

        act.Should().Throw<FormatException>();
    }
}
