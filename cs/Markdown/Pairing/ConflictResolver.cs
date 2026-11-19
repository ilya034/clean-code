using Markdown.Tokens;

namespace Markdown.Pairing;

public static class ConflictResolver
{
    public static List<DroppedConflict> Resolve(IReadOnlyList<Token> tokens, TagPairs pairs)
    {
        var conflicts = new List<DroppedConflict>();

        DropCrossingPairs(tokens, pairs, conflicts);
        DropStrongInsideEmphasis(tokens, pairs, conflicts);

        return conflicts;
    }

    private static void DropCrossingPairs(
        IReadOnlyList<Token> tokens,
        TagPairs pairs,
        List<DroppedConflict> conflicts)
    {
        var openers = new Stack<int>();

        for (var index = 0; index < tokens.Count; index++)
        {
            if (!tokens[index].IsDelimiter)
                continue;

            if (pairs.IsOpening(index))
                openers.Push(index);
            else if (pairs.IsClosing(index))
                CloseThroughCrossed(pairs, openers, pairs.GetPartner(index), index, conflicts);
        }
    }

    private static void CloseThroughCrossed(
        TagPairs pairs,
        Stack<int> openers,
        int openIndex,
        int closeIndex,
        List<DroppedConflict> conflicts)
    {
        List<DelimiterPair>? crossed = null;

        while (openers.Count > 0 && openers.Peek() != openIndex)
        {
            var crossedOpen = openers.Pop();
            crossed ??= [];
            crossed.Add(new DelimiterPair(crossedOpen, pairs.GetPartner(crossedOpen)));
            pairs.Drop(crossedOpen);
        }

        if (openers.Count > 0)
            openers.Pop();

        if (crossed is null)
            return;

        crossed.Add(new DelimiterPair(openIndex, closeIndex));
        pairs.Drop(openIndex);
        conflicts.Add(new DroppedConflict(crossed));
    }

    private static void DropStrongInsideEmphasis(
        IReadOnlyList<Token> tokens,
        TagPairs pairs,
        List<DroppedConflict> conflicts)
    {
        var emphasisDepth = 0;

        for (var index = 0; index < tokens.Count; index++)
        {
            var kind = tokens[index].Kind;

            if (kind == TokenKind.Underscore)
            {
                if (pairs.IsOpening(index))
                    emphasisDepth++;
                else if (pairs.IsClosing(index))
                    emphasisDepth--;
                continue;
            }

            if (kind == TokenKind.DoubleUnderscore && pairs.IsOpening(index) && emphasisDepth > 0)
            {
                var closeIndex = pairs.GetPartner(index);
                pairs.Drop(index);
                conflicts.Add(new DroppedConflict([new DelimiterPair(index, closeIndex)]));
            }
        }
    }
}
