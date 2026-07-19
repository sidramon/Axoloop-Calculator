namespace Domain.Tests.Calculator.TestHelpers;

using Domain.Calculator.Operations;
using Domain.Calculator.Parsing;

public static class ParserFactory
{
    public static Parser CreateDefault()
    {
        var operators = new IOperator[]
        {
            new AddOperator(),
            new SubtractOperator(),
            new MultiplyOperator(),
            new DivideOperator(),
            new ModuloOperator(),
            new PowerOperator(),
            new EqualsOperator(),
            new LessOrEqualOperator(),
            new GreaterOrEqualOperator(),
            new LessOperator(),
            new GreaterOperator(),
        };

        var postfixOperators = new IUnaryOperator[]
        {
            new FactorialOperator(),
        };

        var prefixOperators = new IUnaryOperator[]
        {
            new NegateOperator(),
        };

        return new Parser(operators, postfixOperators, prefixOperators, new Tokenizer());
    }
}
