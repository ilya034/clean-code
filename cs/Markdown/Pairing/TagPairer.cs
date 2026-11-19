using Markdown.Tokens;

namespace Markdown.Pairing;

public class TagPairer
{
    public TagPairs Pair(IReadOnlyList<Token> tokens)
    {
        var whitespace = new WhitespaceIndex(tokens);
        var blockedDelimiters = new bool[tokens.Count];

        while (true)
        {
            var pairs = RunPairingPass(tokens, whitespace, blockedDelimiters);
            var conflicts = ConflictResolver.Resolve(tokens, pairs);

            if (conflicts.Count == 0)
                return pairs;

            var revivedLinks = FindRevivedLinks(tokens, pairs);
            if (revivedLinks.Count == 0)
                return pairs;

            BlockConflictsBlockingLinks(tokens.Count, conflicts, revivedLinks, blockedDelimiters);
        }
    }

    private static TagPairs RunPairingPass(
        IReadOnlyList<Token> tokens,
        WhitespaceIndex whitespace,
        bool[] blockedDelimiters)
    {
        var pairs = new TagPairs(tokens.Count);
        var delimiters = new DelimiterPairing(tokens, whitespace, pairs);
        var links = new LinkPairing(tokens);

        for (var index = 0; index < tokens.Count; index++)
        {
            if (tokens[index].IsDelimiter)
            {
                if (blockedDelimiters[index])
                    continue;

                if (delimiters.PairAt(index) is { } closedOpener)
                    links.NoteClosedDelimiter(closedOpener);

                continue;
            }

            if (links.HandleBracket(index) is { } link)
            {
                pairs.Link(link.OpenIndex, link.CloseIndex);
                delimiters.ForgetOpenersAfter(link.OpenIndex);
            }
        }

        return pairs;
    }

    private static List<Link> FindRevivedLinks(IReadOnlyList<Token> tokens, TagPairs pairs)
    {
        var rescannedLinks = LinkPairing.FindAll(tokens, pairs);
        var revived = new List<Link>();

        foreach (var link in rescannedLinks)
        {
            var alreadyPaired = pairs.TryGetClosingIndex(link.OpenIndex, out var closeIndex)
                                && closeIndex == link.CloseIndex;
            if (!alreadyPaired)
                revived.Add(link);
        }

        return revived;
    }

    private static void BlockConflictsBlockingLinks(
        int tokenCount,
        IReadOnlyList<DroppedConflict> conflicts,
        IReadOnlyList<Link> revivedLinks,
        bool[] blockedDelimiters)
    {
        var revivedLinkOpenAt = BuildRevivedLinkMap(tokenCount, revivedLinks);

        foreach (var conflict in conflicts)
        {
            if (!BlocksAnyRevivedLink(conflict, revivedLinkOpenAt))
                continue;

            foreach (var pair in conflict.Pairs)
            {
                blockedDelimiters[pair.OpenIndex] = true;
                blockedDelimiters[pair.CloseIndex] = true;
            }
        }
    }

    private static int[] BuildRevivedLinkMap(int tokenCount, IReadOnlyList<Link> revivedLinks)
    {
        var linkOpenAt = new int[tokenCount];
        Array.Fill(linkOpenAt, -1);

        foreach (var link in revivedLinks)
            for (var index = link.OpenIndex + 1; index < link.CloseIndex; index++)
                linkOpenAt[index] = link.OpenIndex;

        return linkOpenAt;
    }

    private static bool BlocksAnyRevivedLink(DroppedConflict conflict, int[] revivedLinkOpenAt)
    {
        foreach (var pair in conflict.Pairs)
        {
            var linkOpen = revivedLinkOpenAt[pair.CloseIndex];
            if (linkOpen >= 0 && pair.OpenIndex < linkOpen)
                return true;
        }

        return false;
    }
}
