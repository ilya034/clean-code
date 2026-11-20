using Markdown.Tokens;

namespace Markdown.Pairing;

public class WhitespaceIndex
{
    private readonly int[] prefix;

    public WhitespaceIndex(IReadOnlyList<Token> tokens)
    {
        prefix = new int[tokens.Count + 1];

        for (var index = 0; index < tokens.Count; index++)
            prefix[index + 1] = prefix[index] + (ContainsWhitespace(tokens[index].Value) ? 1 : 0);
    }

    public bool HasWhitespaceBetween(int openIndex, int closeIndex)
    {
        return prefix[closeIndex] - prefix[openIndex + 1] > 0;
    }

    private static bool ContainsWhitespace(string value)
    {
        foreach (var symbol in value)
            if (char.IsWhiteSpace(symbol))
                return true;

        return false;
    }
}