namespace Markdown.Tokens;

public record Token(TokenKind Kind, string Value)
{
    public const char Boundary = '\0';

    public char Before { get; init; } = Boundary;

    public char After { get; init; } = Boundary;

    public bool IsDelimiter => Kind is TokenKind.Underscore or TokenKind.DoubleUnderscore;
}