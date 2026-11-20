using Markdown.Tokens;

namespace Markdown.Pairing;

public class LinkPairing
{
    private readonly Stack<LinkOpener> openers = new();
    private readonly IReadOnlyList<Token> tokens;

    public LinkPairing(IReadOnlyList<Token> tokens)
    {
        this.tokens = tokens;
    }

    public Link? HandleBracket(int index)
    {
        return tokens[index].Kind switch
        {
            TokenKind.LeftBracket => Open(index),
            TokenKind.RightBracket => TryClose(index),
            _ => null
        };
    }

    public void NoteClosedDelimiter(int delimiterOpenIndex)
    {
        if (openers.Count == 0)
            return;

        var opener = openers.Peek();
        opener.MinDelimiterOpenIndex = Math.Min(opener.MinDelimiterOpenIndex, delimiterOpenIndex);
    }

    private Link? Open(int index)
    {
        openers.Push(new LinkOpener(index));
        return null;
    }

    private Link? TryClose(int closeIndex)
    {
        if (openers.Count == 0)
            return null;

        var opener = openers.Pop();
        NoteClosedDelimiter(opener.MinDelimiterOpenIndex);

        var crossesDelimiter = opener.MinDelimiterOpenIndex < opener.OpenIndex;
        var isEmptyText = closeIndex == opener.OpenIndex + 1;

        if (crossesDelimiter || isEmptyText || !HasUrlAfter(closeIndex))
            return null;

        openers.Clear();

        return new Link(opener.OpenIndex, closeIndex);
    }

    private bool HasUrlAfter(int closeIndex)
    {
        return closeIndex + 1 < tokens.Count && tokens[closeIndex + 1].Kind == TokenKind.LinkUrl;
    }

    private class LinkOpener(int openIndex)
    {
        public int OpenIndex { get; } = openIndex;

        public int MinDelimiterOpenIndex { get; set; } = openIndex;
    }
}