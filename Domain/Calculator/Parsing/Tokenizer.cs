namespace Domain.Calculator.Parsing;

using System.Globalization;

public sealed class Tokenizer
{
    private static readonly string[] TwoCharOps = { "<=", ">=", ":=" };
    private static readonly HashSet<string> LogicalKeywords = new() { "and", "or", "not" };

    public IReadOnlyList<Token> Tokenize(string input)
    {
        var tokens = new List<Token>();
        var i = 0;

        while (i < input.Length)
        {
            var c = input[i];

            if (char.IsWhiteSpace(c)) { i++; continue; }

            if (char.IsDigit(c) || c == '.')
            {
                var start = i;
                while (i < input.Length && (char.IsDigit(input[i]) || input[i] == '.')) i++;
                var value = double.Parse(input[start..i], CultureInfo.InvariantCulture);
                tokens.Add(new Token(TokenType.Number, value));
                continue;
            }

            if (char.IsLetter(c) || c == '_')
            {
                var start = i;
                while (i < input.Length && (char.IsLetterOrDigit(input[i]) || input[i] == '_')) i++;
                var word = input[start..i];
                tokens.Add(LogicalKeywords.Contains(word)
                    ? new Token(TokenType.Operator, Symbol: word)
                    : new Token(TokenType.Identifier, Symbol: word));
                continue;
            }

            switch (c)
            {
                case '(': tokens.Add(new Token(TokenType.LeftParen));    i++; continue;
                case ')': tokens.Add(new Token(TokenType.RightParen));   i++; continue;
                case '[': tokens.Add(new Token(TokenType.LeftBracket));  i++; continue;
                case ']': tokens.Add(new Token(TokenType.RightBracket)); i++; continue;
                case ',': tokens.Add(new Token(TokenType.Comma));        i++; continue;
                case ';': tokens.Add(new Token(TokenType.Semicolon));    i++; continue;
            }

            if (i + 1 < input.Length)
            {
                var pair = input.Substring(i, 2);
                if (Array.IndexOf(TwoCharOps, pair) >= 0)
                {
                    tokens.Add(new Token(TokenType.Operator, Symbol: pair));
                    i += 2;
                    continue;
                }
            }

            tokens.Add(new Token(TokenType.Operator, Symbol: c.ToString()));
            i++;
        }

        return tokens;
    }
}