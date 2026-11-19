using Markdown.Tokens;

namespace Markdown.Pairing;

public class DelimiterPairing
{
    private readonly IReadOnlyList<Token> tokens;
    private readonly WhitespaceIndex whitespace;
    private readonly TagPairs pairs;
    private readonly OpenerStacks openers = new();

    public DelimiterPairing(IReadOnlyList<Token> tokens, WhitespaceIndex whitespace, TagPairs pairs)
    {
        this.tokens = tokens;
        this.whitespace = whitespace;
        this.pairs = pairs;
    }

    public int? PairAt(int index)
    {
        var token = tokens[index];
        var sameKind = openers.For(token.Kind);

        if (token.CanClose())
        {
            ForgetExhausted(sameKind, index);

            if (sameKind.Count > 0)
            {
                var openIndex = sameKind.Peek();

                if (index == openIndex + 1)
                {
                    sameKind.Pop();
                    return null;
                }

                if (CanPair(openIndex, index))
                {
                    sameKind.Pop();
                    pairs.Link(openIndex, index);
                    return openIndex;
                }
            }
        }

        if (token.CanOpen())
            sameKind.Push(index);

        return null;
    }

    public void ForgetOpenersAfter(int linkOpenIndex)
    {
        openers.ForgetAfter(linkOpenIndex);
    }

    private void ForgetExhausted(Stack<int> sameKind, int closeIndex)
    {
        while (sameKind.Count > 0 && IsExhausted(sameKind.Peek(), closeIndex))
            sameKind.Pop();
    }

    private bool IsExhausted(int openIndex, int closeIndex)
    {
        return whitespace.HasWhitespaceBetween(openIndex, closeIndex) && tokens[openIndex].IsInsideWord();
    }

    private bool CanPair(int openIndex, int closeIndex)
    {
        if (!whitespace.HasWhitespaceBetween(openIndex, closeIndex))
            return true;

        return !tokens[openIndex].IsInsideWord() && !tokens[closeIndex].IsInsideWord();
    }

    private class OpenerStacks
    {
        private readonly Stack<int> emphasis = new();
        private readonly Stack<int> strong = new();

        public Stack<int> For(TokenKind kind)
        {
            return kind == TokenKind.Underscore ? emphasis : strong;
        }

        public void ForgetAfter(int linkOpenIndex)
        {
            ForgetAfter(emphasis, linkOpenIndex);
            ForgetAfter(strong, linkOpenIndex);
        }

        private static void ForgetAfter(Stack<int> stack, int linkOpenIndex)
        {
            while (stack.Count > 0 && stack.Peek() > linkOpenIndex)
                stack.Pop();
        }
    }
}
