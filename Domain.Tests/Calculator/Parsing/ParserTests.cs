namespace Domain.Tests.Calculator.Parsing;

using Domain.Calculator;
using Domain.Calculator.Ast;
using Domain.Calculator.Values;
using Domain.Tests.Calculator.TestHelpers;
using FluentAssertions;
using Value = Domain.Calculator.Values.Value;

public class ParserTests
{
    private readonly Domain.Calculator.Parsing.Parser _parser = ParserFactory.CreateDefault();
    private readonly Evaluator _evaluator = EvaluatorFactory.CreateDefault();

    private Value Evaluate(string input, VariableContext? context = null)
    {
        context ??= new VariableContext();
        var ast = _parser.Parse(input);
        return _evaluator.Evaluate(ast, context);
    }

    private double EvaluateNumber(string input, VariableContext? context = null) =>
        ((NumberValue)Evaluate(input, context)).Number;

    // ---- Precedence ----

    [Theory]
    [InlineData("2+3*4", 14)]
    [InlineData("2*3+4", 10)]
    [InlineData("(2+3)*4", 20)]
    public void Parse_ArithmeticPrecedence_EvaluatesInCorrectOrder(string input, double expected)
    {
        EvaluateNumber(input).Should().BeApproximately(expected, 1e-10);
    }

    [Fact]
    public void Parse_ComparisonHasLowerPrecedenceThanAddition_EvaluatesArithmeticFirst()
    {
        var result = Evaluate("1+2=3");

        result.Should().BeOfType<BooleanValue>();
        ((BooleanValue)result).Boolean.Should().BeTrue();
    }

    // ---- Associativity ----

    [Fact]
    public void Parse_Power_IsRightAssociative()
    {
        EvaluateNumber("2^3^2").Should().BeApproximately(512, 1e-10);
    }

    // ---- Unary minus ----

    [Fact]
    public void Parse_UnaryMinusBindsTighterThanPowerOperand_NegatesAfterExponentiation()
    {
        EvaluateNumber("-2^2").Should().BeApproximately(-4, 1e-10);
    }

    [Fact]
    public void Parse_ParenthesizedNegation_SquaresTheNegativeValue()
    {
        EvaluateNumber("(-2)^2").Should().BeApproximately(4, 1e-10);
    }

    [Fact]
    public void Parse_MultiplicationFollowedByUnaryMinus_AppliesNegationToRightOperand()
    {
        EvaluateNumber("3*-2").Should().BeApproximately(-6, 1e-10);
    }

    [Fact]
    public void Parse_LeadingUnaryMinus_NegatesTheNumber()
    {
        EvaluateNumber("-5").Should().BeApproximately(-5, 1e-10);
    }

    // ---- Postfix factorial ----

    [Fact]
    public void Parse_Factorial_EvaluatesToProductOfIntegers()
    {
        EvaluateNumber("5!").Should().BeApproximately(120, 1e-10);
    }

    [Fact]
    public void Parse_NegatedFactorial_NegatesAfterFactorial()
    {
        EvaluateNumber("-3!").Should().BeApproximately(-6, 1e-10);
    }

    [Fact]
    public void Parse_ParenthesizedExpressionFactorial_EvaluatesParenthesesFirst()
    {
        EvaluateNumber("(2+1)!").Should().BeApproximately(6, 1e-10);
    }

    // ---- Parentheses ----

    [Fact]
    public void Parse_NestedParentheses_EvaluatesInnermostFirst()
    {
        EvaluateNumber("((1+2)*(3+4))").Should().BeApproximately(21, 1e-10);
    }

    [Fact]
    public void Parse_MissingClosingParenthesis_ThrowsFormatException()
    {
        var act = () => _parser.Parse("(1+2");

        act.Should().Throw<FormatException>();
    }

    // ---- Functions ----

    [Fact]
    public void Parse_FunctionWithCorrectArity_Evaluates()
    {
        EvaluateNumber("sqrt(9)").Should().BeApproximately(3, 1e-10);
    }

    [Fact]
    public void Evaluate_FunctionWithIncorrectArity_ThrowsInvalidOperationException()
    {
        // Arity is no longer validated during parsing: any 'identifier(...)' becomes a
        // generic CallExpression, and arity is checked by the Evaluator once the name
        // is resolved against special forms / builtins / user functions.
        var act = () => Evaluate("sqrt(1,2)");

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Parse_NestedFunctionCalls_EvaluatesInnerFunctionFirst()
    {
        EvaluateNumber("sqrt(abs(-4))").Should().BeApproximately(2, 1e-10);
    }

    [Fact]
    public void Parse_MultiArgumentFunction_Pow_Evaluates()
    {
        EvaluateNumber("pow(2,10)").Should().BeApproximately(1024, 1e-10);
    }

    [Fact]
    public void Parse_MultiArgumentFunction_DotProduct_Evaluates()
    {
        EvaluateNumber("dotp([1,2],[3,4])").Should().BeApproximately(11, 1e-10);
    }

    [Fact]
    public void Parse_MultiArgumentFunction_ReshapeOnVariable_Evaluates()
    {
        var context = new VariableContext();
        Evaluate("m := [1,2,3,4,5,6]", context);

        var result = (MatrixValue)Evaluate("reshape(m,2,3)", context);

        result.Rows.Should().Be(2);
        result.Columns.Should().Be(3);
        result[1, 2].Should().BeApproximately(6, 1e-10);
    }

    // ---- Matrices ----

    [Fact]
    public void Parse_MatrixLiteral_ProducesExpectedShape()
    {
        var result = (MatrixValue)Evaluate("[1,2;3,4]");

        result[0, 0].Should().BeApproximately(1, 1e-10);
        result[0, 1].Should().BeApproximately(2, 1e-10);
        result[1, 0].Should().BeApproximately(3, 1e-10);
        result[1, 1].Should().BeApproximately(4, 1e-10);
    }

    [Fact]
    public void Parse_MatrixWithUnequalRowLengths_ThrowsFormatException()
    {
        var act = () => _parser.Parse("[1,2;3]");

        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void Parse_MatrixCellsAsExpressions_EvaluatesEachCell()
    {
        var result = (MatrixValue)Evaluate("[1+1, sqrt(9)]");

        result[0, 0].Should().BeApproximately(2, 1e-10);
        result[0, 1].Should().BeApproximately(3, 1e-10);
    }

    // ---- Assignment ----

    [Fact]
    public void Parse_Assignment_ProducesAssignmentExpression()
    {
        var ast = _parser.Parse("a := 5");

        ast.Should().BeOfType<AssignmentExpression>();
        ((AssignmentExpression)ast).Name.Should().Be("a");
    }

    [Fact]
    public void Parse_NestedAssignment_ThrowsFormatException()
    {
        var act = () => _parser.Parse("a := b := 5");

        act.Should().Throw<FormatException>();
    }

    // ---- Trailing input ----

    [Fact]
    public void Parse_TrailingInputAfterCompleteExpression_ThrowsFormatException()
    {
        var act = () => _parser.Parse("1 2");

        act.Should().Throw<FormatException>();
    }

    // ---- Less / Greater ----

    [Theory]
    [InlineData("3 < 5", true)]
    [InlineData("5 < 3", false)]
    [InlineData("5 > 5", false)]
    [InlineData("6 > 5", true)]
    public void Parse_LessAndGreaterOperators_Evaluate(string input, bool expected)
    {
        var result = (BooleanValue)Evaluate(input);

        result.Boolean.Should().Be(expected);
    }

    // ---- Logical operators: precedence crossing arithmetic/comparison/logic ----

    [Fact]
    public void Parse_ComparisonBindsTighterThanAnd_EvaluatesComparisonsFirst()
    {
        var result = (BooleanValue)Evaluate("1 < 2 and 3 > 2");

        result.Boolean.Should().BeTrue();
    }

    [Fact]
    public void Parse_ComparisonBindsTighterThanOr_EvaluatesComparisonsFirst()
    {
        var result = (BooleanValue)Evaluate("1 > 2 or 2 > 1");

        result.Boolean.Should().BeTrue();
    }

    [Fact]
    public void Parse_NotBindsTighterThanComparisonIsFalse_ButLooserThanComparisonItself()
    {
        // "not 1 > 2" reads as "not (1 > 2)": comparison binds tighter than 'not'.
        var result = (BooleanValue)Evaluate("not 1 > 2");

        result.Boolean.Should().BeTrue();
    }

    [Fact]
    public void Parse_AndBindsTighterThanOr_EvaluatesAsAndFirst()
    {
        // "1 < 2 and 2 < 3 or 5 < 1" reads as "(1 < 2 and 2 < 3) or (5 < 1)".
        var result = (BooleanValue)Evaluate("1 < 2 and 2 < 3 or 5 < 1");

        result.Boolean.Should().BeTrue();
    }

    [Fact]
    public void Parse_OrIsLeftAssociative()
    {
        // "1 > 2 or 1 > 2 or 3 > 2" only holds if grouped left-to-right.
        var result = (BooleanValue)Evaluate("1 > 2 or 1 > 2 or 3 > 2");

        result.Boolean.Should().BeTrue();
    }

    // ---- Function definition parsing ----

    [Fact]
    public void Parse_FunctionDefinitionWithOneParameter_ProducesFunctionDefinitionExpression()
    {
        var ast = _parser.Parse("y(x) := 2*x + 5");

        ast.Should().BeOfType<FunctionDefinitionExpression>();
        var definition = (FunctionDefinitionExpression)ast;
        definition.Name.Should().Be("y");
        definition.Parameters.Should().Equal("x");
    }

    [Fact]
    public void Parse_FunctionDefinitionWithZeroParameters_ProducesEmptyParameterList()
    {
        var ast = (FunctionDefinitionExpression)_parser.Parse("k() := 5");

        ast.Parameters.Should().BeEmpty();
    }

    [Fact]
    public void Parse_FunctionDefinitionWithMultipleParameters_ProducesOrderedParameterList()
    {
        var ast = (FunctionDefinitionExpression)_parser.Parse("f(a,b) := sqrt(a^2 + b^2)");

        ast.Parameters.Should().Equal("a", "b");
    }

    [Fact]
    public void Parse_FunctionDefinitionWithDuplicateParameterNames_ThrowsFormatException()
    {
        var act = () => _parser.Parse("f(a,a) := a");

        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void Parse_CallWithoutPriorDefinition_ProducesCallExpressionNotDefinition()
    {
        var ast = _parser.Parse("y(3)");

        ast.Should().BeOfType<CallExpression>();
        ((CallExpression)ast).Name.Should().Be("y");
    }

    [Fact]
    public void Parse_IdentifierFollowedByParensWithoutAssignment_DoesNotBacktrackIntoDefinition()
    {
        // "(a)" after the identifier's parens looks structurally close to a definition
        // but there is no ':=', so this must resolve as a plain call.
        var ast = _parser.Parse("f(1)");

        ast.Should().BeOfType<CallExpression>();
    }

    // ---- Chained calls: f(2)(3) ----

    [Fact]
    public void Parse_OrdinaryCall_StaysACallExpression_NotWrappedInInvoke()
    {
        var ast = _parser.Parse("sqrt(abs(-4))");

        ast.Should().BeOfType<CallExpression>();
        ((CallExpression)ast).Arguments[0].Should().BeOfType<CallExpression>();
    }

    [Fact]
    public void Parse_SecondParenAfterACall_ProducesInvokeExpressionWrappingTheCall()
    {
        var ast = _parser.Parse("f(2)(3)");

        ast.Should().BeOfType<InvokeExpression>();
        var invoke = (InvokeExpression)ast;
        invoke.Target.Should().BeOfType<CallExpression>();
        ((CallExpression)invoke.Target).Name.Should().Be("f");
        invoke.Arguments.Should().ContainSingle();
    }

    [Fact]
    public void Evaluate_ChainedCall_InvokesTheFunctionValueReturnedByTheFirstCall()
    {
        var context = new VariableContext();
        Evaluate("g(y) := y + 1", context);
        Evaluate("f(x) := g", context); // f ignores its argument and returns g itself

        EvaluateNumber("f(2)(3)", context).Should().BeApproximately(4, 1e-10);
    }

    [Fact]
    public void Evaluate_ChainOfThreeCalls_InvokesEachReturnedFunctionInTurn()
    {
        var context = new VariableContext();
        Evaluate("h(z) := z * 10", context);
        Evaluate("g(y) := h", context);
        Evaluate("f(x) := g", context);

        EvaluateNumber("f(1)(2)(3)", context).Should().BeApproximately(30, 1e-10);
    }

    [Fact]
    public void Evaluate_InvokingANonFunctionValue_ThrowsMentioningTheActualType()
    {
        var context = new VariableContext();
        Evaluate("f(x) := x + 1", context); // f(2) is a plain number, not invokable

        var act = () => Evaluate("f(2)(3)", context);

        act.Should().Throw<InvalidOperationException>().WithMessage("*annot invoke*number*");
    }

    [Fact]
    public void Evaluate_ChainedCallFollowedByFactorial_AppliesInvocationBeforePostfix()
    {
        var context = new VariableContext();
        Evaluate("g(y) := y", context);
        Evaluate("f(x) := g", context);

        // f(1)(4)! must read as ((f(1))(4))! = 4! = 24, not some other grouping.
        EvaluateNumber("f(1)(4)!", context).Should().BeApproximately(24, 1e-10);
    }

    [Fact]
    public void Evaluate_SpecialFormResultFollowedByInvocation_InvokesTheChosenBranch()
    {
        var context = new VariableContext();
        Evaluate("g(y) := y * 2", context);

        EvaluateNumber("if(1 < 2, g, g)(5)", context).Should().BeApproximately(10, 1e-10);
    }

    [Fact]
    public void Evaluate_SpecialFormResultFollowedByInvocation_NonFunctionBranchThrows()
    {
        var context = new VariableContext();

        var act = () => Evaluate("if(1 < 2, 1, 2)(5)", context);

        act.Should().Throw<InvalidOperationException>().WithMessage("*annot invoke*number*");
    }
}
