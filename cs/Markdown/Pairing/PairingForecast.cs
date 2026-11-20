using Markdown.Tokens;

namespace Markdown.Pairing;

public sealed class PairingPrediction
{
    private const int NoPartner = -1;

    private readonly int[] closers;

    private PairingPrediction(int[] closers)
    {
        this.closers = closers;
    }
    
    public static PairingPrediction Build(IReadOnlyList<Token> tokens, WhitespaceIndex whitespace)
    {
        var closers = Find(tokens, whitespace, links: false);
        var optimistic = Find(tokens, whitespace, links: true);

        for (var index = 0; index < closers.Length; index++)
            closers[index] = Math.Max(closers[index], optimistic[index]);

        return new PairingPrediction(closers);
    }

    public bool WillClose(int openerIndex)
    {
        return closers[openerIndex] > openerIndex;
    }

    private static int[] Find(IReadOnlyList<Token> tokens, WhitespaceIndex whitespace, bool links)
    {
        var closers = new int[tokens.Count];
        Array.Fill(closers, NoPartner);

        var emphasis = new Stack<int>();
        var strong = new Stack<int>();
        var brackets = new Stack<int>();

        for (var index = 0; index < tokens.Count; index++)
        {
            var token = tokens[index];

            if (token.Kind == TokenKind.LeftBracket)
            {
                brackets.Push(index);
                continue;
            }

            if (token.Kind == TokenKind.RightBracket)
            {
                if (links)
                    CloseLink(tokens, brackets, emphasis, strong, index);

                continue;
            }

            if (!token.IsDelimiter)
                continue;

            var sameKind = token.Kind == TokenKind.Underscore ? emphasis : strong;

            if (token.CanClose() && TryPair(tokens, whitespace, sameKind, index, closers))
                continue;

            if (token.CanOpen())
                sameKind.Push(index);
        }

        return closers;
    }

    private static bool TryPair(
        IReadOnlyList<Token> tokens,
        WhitespaceIndex whitespace,
        Stack<int> sameKind,
        int index,
        int[] closers)
    {
        while (sameKind.Count > 0 && PairingRules.IsExhausted(tokens, whitespace, sameKind.Peek(), index))
            sameKind.Pop();

        if (sameKind.Count == 0)
            return false;

        var openIndex = sameKind.Peek();

        if (index == openIndex + 1)
        {
            sameKind.Pop();
            return true;
        }

        if (!PairingRules.CanPair(tokens, whitespace, openIndex, index))
            return false;

        sameKind.Pop();
        closers[openIndex] = index;
        closers[index] = openIndex;

        return true;
    }

    private static void CloseLink(
        IReadOnlyList<Token> tokens,
        Stack<int> brackets,
        Stack<int> emphasis,
        Stack<int> strong,
        int closeIndex)
    {
        if (brackets.Count == 0)
            return;

        var openIndex = brackets.Pop();

        if (closeIndex == openIndex + 1 || closeIndex + 1 >= tokens.Count)
            return;

        if (tokens[closeIndex + 1].Kind != TokenKind.LinkUrl)
            return;

        brackets.Clear();
        ForgetAbove(emphasis, openIndex);
        ForgetAbove(strong, openIndex);
    }

    private static void ForgetAbove(Stack<int> openers, int openIndex)
    {
        while (openers.Count > 0 && openers.Peek() > openIndex)
            openers.Pop();
    }
}
