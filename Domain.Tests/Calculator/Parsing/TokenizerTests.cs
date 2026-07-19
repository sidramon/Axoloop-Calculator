namespace Domain.Tests.Calculator.Parsing;

using Domain.Calculator.Parsing;
using FluentAssertions;

public class TokenizerTests
{
    private readonly Tokenizer _tokenizer = new();

    [Theory]
    [InlineData(":=")]
    [InlineData("<=")]
    [InlineData(">=")]
    public void Tokenize_TwoCharacterOperator_ProducesSingleOperatorToken(string op)
    {
        var tokens = _tokenizer.Tokenize(op);

        tokens.Should().ContainSingle();
        tokens[0].Type.Should().Be(TokenType.Operator);
        tokens[0].Symbol.Should().Be(op);
    }

    [Theory]
    [InlineData("_pi")]
    [InlineData("my_var")]
    [InlineData("x1")]
    public void Tokenize_IdentifierWithUnderscoreOrDigit_ProducesSingleIdentifierToken(string identifier)
    {
        var tokens = _tokenizer.Tokenize(identifier);

        tokens.Should().ContainSingle();
        tokens[0].Type.Should().Be(TokenType.Identifier);
        tokens[0].Symbol.Should().Be(identifier);
    }

    [Fact]
    public void Tokenize_DecimalNumber_ProducesNumberToken()
    {
        var tokens = _tokenizer.Tokenize("3.14");

        tokens.Should().ContainSingle();
        tokens[0].Type.Should().Be(TokenType.Number);
        tokens[0].Number.Should().BeApproximately(3.14, 1e-10);
    }

    [Fact]
    public void Tokenize_MatrixSeparators_ProducesExpectedTokenTypes()
    {
        var tokens = _tokenizer.Tokenize("[1,2;3]");

        tokens.Select(t => t.Type).Should().Equal(
            TokenType.LeftBracket,
            TokenType.Number,
            TokenType.Comma,
            TokenType.Number,
            TokenType.Semicolon,
            TokenType.Number,
            TokenType.RightBracket);
    }

    [Fact]
    public void Tokenize_WhitespaceBetweenTokens_IsIgnored()
    {
        var tokens = _tokenizer.Tokenize("  1   +   2  ");

        tokens.Select(t => t.Type).Should().Equal(TokenType.Number, TokenType.Operator, TokenType.Number);
    }

    [Theory]
    [InlineData("<")]
    [InlineData(">")]
    public void Tokenize_SingleCharacterComparisonOperator_ProducesOperatorToken(string op)
    {
        var tokens = _tokenizer.Tokenize(op);

        tokens.Should().ContainSingle();
        tokens[0].Type.Should().Be(TokenType.Operator);
        tokens[0].Symbol.Should().Be(op);
    }

    [Fact]
    public void Tokenize_LessOrEqual_IsNotMisreadAsLessFollowedByEquals()
    {
        var tokens = _tokenizer.Tokenize("a <= b");

        tokens.Select(t => t.Type).Should().Equal(
            TokenType.Identifier, TokenType.Operator, TokenType.Identifier);
        tokens[1].Symbol.Should().Be("<=");
    }

    [Theory]
    [InlineData("and")]
    [InlineData("or")]
    [InlineData("not")]
    public void Tokenize_LogicalKeyword_ProducesOperatorTokenNotIdentifier(string keyword)
    {
        var tokens = _tokenizer.Tokenize(keyword);

        tokens.Should().ContainSingle();
        tokens[0].Type.Should().Be(TokenType.Operator);
        tokens[0].Symbol.Should().Be(keyword);
    }

    [Theory]
    [InlineData("And")]
    [InlineData("AND")]
    [InlineData("andCondition")]
    public void Tokenize_WordResemblingLogicalKeywordButNotExactMatch_ProducesIdentifierToken(string word)
    {
        var tokens = _tokenizer.Tokenize(word);

        tokens.Should().ContainSingle();
        tokens[0].Type.Should().Be(TokenType.Identifier);
        tokens[0].Symbol.Should().Be(word);
    }
}
