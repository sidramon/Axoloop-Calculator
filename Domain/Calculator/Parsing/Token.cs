namespace Domain.Calculator.Parsing;

public enum TokenType
{
    Number, Identifier, Operator,
    LeftParen, RightParen,
    LeftBracket, RightBracket,
    Comma, Semicolon
}

public readonly record struct Token(TokenType Type, double Number = 0, string Symbol = "");