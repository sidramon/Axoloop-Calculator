namespace Domain.Calculator.Parsing;

using Domain.Calculator.Ast;
using Domain.Calculator.Operations;
using Domain.Calculator.Values;

public sealed class Parser
{
    private readonly IReadOnlyDictionary<string, IOperator> _operators;
    private readonly IReadOnlyDictionary<string, IUnaryOperator> _postfixOperators;
    private readonly IReadOnlyDictionary<string, IUnaryOperator> _prefixOperators;
    private readonly Tokenizer _tokenizer;

    private const int OrPrecedence = 1;
    private const int AndPrecedence = 2;
    private const int NotOperandPrecedence = 3;
    private const int PrefixOperandPrecedence = 6;

    public Parser(
        IEnumerable<IOperator> operators,
        IEnumerable<IUnaryOperator> postfixOperators,
        IEnumerable<IUnaryOperator> prefixOperators,
        Tokenizer tokenizer)
    {
        _operators = operators.ToDictionary(o => o.Symbol);
        _postfixOperators = postfixOperators.ToDictionary(o => o.Symbol);
        _prefixOperators = prefixOperators.ToDictionary(o => o.Symbol);
        _tokenizer = tokenizer;
    }

    public IExpression Parse(string input)
    {
        var tokens = _tokenizer.Tokenize(input);
        var position = 0;
        var expression = ParseStatement(tokens, ref position);

        if (position < tokens.Count)
            throw new FormatException("Unexpected trailing input.");

        return expression;
    }

    private IExpression ParseStatement(IReadOnlyList<Token> tokens, ref int position)
    {
        var functionDefinition = TryParseFunctionDefinition(tokens, ref position);
        if (functionDefinition is not null)
            return functionDefinition;

        if (position + 1 < tokens.Count
            && tokens[position].Type == TokenType.Identifier
            && tokens[position + 1].Type == TokenType.Operator
            && tokens[position + 1].Symbol == ":=")
        {
            var name = tokens[position].Symbol;
            position += 2;
            var value = ParseExpression(tokens, ref position, 0);
            return new AssignmentExpression(name, value);
        }

        return ParseExpression(tokens, ref position, 0);
    }

    private FunctionDefinitionExpression? TryParseFunctionDefinition(IReadOnlyList<Token> tokens, ref int position)
    {
        if (position >= tokens.Count || tokens[position].Type != TokenType.Identifier)
            return null;

        var name = tokens[position].Symbol;
        var cursor = position + 1;

        if (cursor >= tokens.Count || tokens[cursor].Type != TokenType.LeftParen)
            return null;
        cursor++;

        var parameters = new List<string>();
        if (cursor < tokens.Count && tokens[cursor].Type != TokenType.RightParen)
        {
            while (true)
            {
                if (cursor >= tokens.Count || tokens[cursor].Type != TokenType.Identifier)
                    return null;
                parameters.Add(tokens[cursor].Symbol);
                cursor++;

                if (cursor < tokens.Count && tokens[cursor].Type == TokenType.Comma)
                {
                    cursor++;
                    continue;
                }
                break;
            }
        }

        if (cursor >= tokens.Count || tokens[cursor].Type != TokenType.RightParen)
            return null;
        cursor++;

        if (cursor >= tokens.Count || tokens[cursor].Type != TokenType.Operator || tokens[cursor].Symbol != ":=")
            return null;
        cursor++;

        if (parameters.Distinct().Count() != parameters.Count)
            throw new FormatException($"Duplicate parameter name in definition of '{name}'.");

        position = cursor;
        var body = ParseExpression(tokens, ref position, 0);
        return new FunctionDefinitionExpression(name, parameters, body);
    }

    private IExpression ParseExpression(IReadOnlyList<Token> tokens, ref int position, int minPrecedence)
    {
        var left = ParseUnaryPrefix(tokens, ref position);

        while (position < tokens.Count && tokens[position].Type == TokenType.Operator)
        {
            var symbol = tokens[position].Symbol;

            if (symbol is "and" or "or")
            {
                var precedence = symbol == "and" ? AndPrecedence : OrPrecedence;
                if (precedence < minPrecedence) break;

                position++;
                var logicalRight = ParseExpression(tokens, ref position, precedence + 1);
                left = new LogicalExpression(
                    left,
                    symbol == "and" ? LogicalOperator.And : LogicalOperator.Or,
                    logicalRight);
                continue;
            }

            if (!_operators.TryGetValue(symbol, out var op))
                break;

            if (op.Precedence < minPrecedence) break;

            position++;
            var nextMin = op.Associativity == Associativity.Left
                ? op.Precedence + 1
                : op.Precedence;
            var right = ParseExpression(tokens, ref position, nextMin);
            left = new BinaryExpression(left, op, right);
        }

        return left;
    }

    private IExpression ParseUnaryPostfix(IReadOnlyList<Token> tokens, ref int position)
    {
        var operand = ParsePrimary(tokens, ref position);

        while (true)
        {
            if (position < tokens.Count && tokens[position].Type == TokenType.LeftParen)
            {
                var arguments = ParseArgumentList(tokens, ref position, "a chained call");
                operand = new InvokeExpression(operand, arguments);
                continue;
            }

            if (position < tokens.Count
                && tokens[position].Type == TokenType.Operator
                && _postfixOperators.TryGetValue(tokens[position].Symbol, out var postfix))
            {
                position++;
                operand = new UnaryExpression(operand, postfix);
                continue;
            }

            break;
        }

        return operand;
    }

    private IReadOnlyList<IExpression> ParseArgumentList(IReadOnlyList<Token> tokens, ref int position, string context)
    {
        position++; // consume '('

        var arguments = new List<IExpression>();
        if (position < tokens.Count && tokens[position].Type != TokenType.RightParen)
        {
            while (true)
            {
                arguments.Add(ParseExpression(tokens, ref position, 0));

                if (position < tokens.Count && tokens[position].Type == TokenType.Comma)
                {
                    position++;
                    continue;
                }
                break;
            }
        }

        if (position >= tokens.Count || tokens[position].Type != TokenType.RightParen)
            throw new FormatException($"Missing closing parenthesis in {context}.");
        position++;

        return arguments;
    }

    private IExpression ParseUnaryPrefix(IReadOnlyList<Token> tokens, ref int position)
    {
        if (position < tokens.Count
            && tokens[position].Type == TokenType.Operator
            && tokens[position].Symbol == "not")
        {
            position++;
            var operand = ParseExpression(tokens, ref position, NotOperandPrecedence);
            return new NotExpression(operand);
        }

        if (position < tokens.Count
            && tokens[position].Type == TokenType.Operator
            && _prefixOperators.TryGetValue(tokens[position].Symbol, out var prefix))
        {
            position++;
            var operand = ParseExpression(tokens, ref position, PrefixOperandPrecedence);
            return new UnaryExpression(operand, prefix);
        }

        return ParseUnaryPostfix(tokens, ref position);
    }

    private IExpression ParsePrimary(IReadOnlyList<Token> tokens, ref int position)
    {
        if (position >= tokens.Count)
            throw new FormatException("Unexpected end of expression.");

        var token = tokens[position];

        switch (token.Type)
        {
            case TokenType.Number:
                position++;
                return new NumberExpression(new NumberValue(token.Number));

            case TokenType.Identifier:
            {
                var name = token.Symbol;
                position++;

                if (position >= tokens.Count || tokens[position].Type != TokenType.LeftParen)
                    return new IdentifierExpression(name);

                var args = ParseArgumentList(tokens, ref position, $"'{name}'");
                return new CallExpression(name, args);
            }

            case TokenType.LeftParen:
                position++;
                var inner = ParseExpression(tokens, ref position, 0);
                if (position >= tokens.Count || tokens[position].Type != TokenType.RightParen)
                    throw new FormatException("Missing closing parenthesis.");
                position++;
                return inner;

            case TokenType.LeftBracket:
                return ParseMatrix(tokens, ref position);

            default:
                throw new FormatException($"Unexpected token '{token.Symbol}'.");
        }
    }

    private IExpression ParseMatrix(IReadOnlyList<Token> tokens, ref int position)
    {
        position++; // consume '['

        var rows = new List<IReadOnlyList<IExpression>>();
        var current = new List<IExpression>();

        while (position < tokens.Count && tokens[position].Type != TokenType.RightBracket)
        {
            current.Add(ParseExpression(tokens, ref position, 0));

            if (position >= tokens.Count)
                throw new FormatException("Unterminated matrix.");

            switch (tokens[position].Type)
            {
                case TokenType.Comma:
                    position++;
                    break;

                case TokenType.Semicolon:
                    position++;
                    rows.Add(current);
                    current = new List<IExpression>();
                    break;

                case TokenType.RightBracket:
                    break;

                default:
                    throw new FormatException("Expected ',', ';' or ']' in matrix.");
            }
        }

        if (position >= tokens.Count)
            throw new FormatException("Missing closing ']'.");
        position++; // consume ']'

        rows.Add(current);

        var width = rows[0].Count;
        if (rows.Any(r => r.Count != width))
            throw new FormatException("All matrix rows must have the same length.");

        return new MatrixExpression(rows);
    }
}
