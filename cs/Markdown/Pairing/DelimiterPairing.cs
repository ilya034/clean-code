using Markdown.Tokens;

namespace Markdown.Pairing;

public class DelimiterPairing
{
    private readonly IReadOnlyList<Token> tokens;
    private readonly WhitespaceIndex whitespace;
    private readonly TagPairs pairs;
    private readonly PairingPrediction forecast;
    private readonly Openers openers;

    public DelimiterPairing(IReadOnlyList<Token> tokens, WhitespaceIndex whitespace, TagPairs pairs)
    {
        this.tokens = tokens;
        this.whitespace = whitespace;
        this.pairs = pairs;
        forecast = PairingPrediction.Build(tokens, whitespace);
        openers = new Openers(tokens.Count);
    }

    public int? PairAt(int index)
    {
        var token = tokens[index];

        if (token.CanClose())
        {
            ForgetExhausted(token.Kind, index);

            if (openers.Count(token.Kind) > 0)
            {
                var openIndex = openers.Peek(token.Kind);

                if (index == openIndex + 1)
                {
                    openers.Pop(token.Kind);
                    return null;
                }

                if (CanPair(openIndex, index))
                {
                    openers.Pop(token.Kind);
                    return Link(token.Kind, openIndex, index);
                }
            }
        }

        if (token.CanOpen())
            openers.Push(token.Kind, index, forecast.WillClose(index));

        return null;
    }

    public void ForgetOpenersAfter(int linkOpenIndex)
    {
        openers.ForgetAbove(TokenKind.Underscore, linkOpenIndex);
        openers.ForgetAbove(TokenKind.DoubleUnderscore, linkOpenIndex);
    }

    private int? Link(TokenKind kind, int openIndex, int closeIndex)
    {
        if (openers.IsDoomed(openIndex))
            return null;

        if (openers.DoomVisibleAbove(PairingRules.Other(kind), openIndex))
            return null;

        if (kind == TokenKind.DoubleUnderscore && openers.HasVisible(TokenKind.Underscore))
            return null;

        pairs.Link(openIndex, closeIndex);
        return openIndex;
    }

    private void ForgetExhausted(TokenKind kind, int closeIndex)
    {
        while (openers.Count(kind) > 0 && IsExhausted(openers.Peek(kind), closeIndex))
            openers.Pop(kind);
    }

    private bool IsExhausted(int openIndex, int closeIndex)
    {
        return PairingRules.IsExhausted(tokens, whitespace, openIndex, closeIndex);
    }

    private bool CanPair(int openIndex, int closeIndex)
    {
        return PairingRules.CanPair(tokens, whitespace, openIndex, closeIndex);
    }
}
