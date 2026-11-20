using Markdown.Tokens;

namespace Markdown.Pairing;

public class TagPairer
{
    public TagPairs Pair(IReadOnlyList<Token> tokens)
    {
        var pairs = new TagPairs(tokens.Count);
        var whitespace = new WhitespaceIndex(tokens);
        var delimiters = new DelimiterPairing(tokens, whitespace, pairs);
        var links = new LinkPairing(tokens);

        for (var index = 0; index < tokens.Count; index++)
        {
            var token = tokens[index];

            if (token.IsDelimiter)
            {
                if (delimiters.PairAt(index) is { } openIndex)
                    links.NoteClosedDelimiter(openIndex);

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
}