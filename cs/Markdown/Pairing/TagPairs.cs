namespace Markdown.Pairing;

public class TagPairs
{
    private const int NoPair = -1;
    private readonly int[] partners;

    public TagPairs(int tokenCount)
    {
        partners = new int[tokenCount];
        Array.Fill(partners, NoPair);
    }

    public bool TryGetClosingIndex(int openIndex, out int closeIndex)
    {
        closeIndex = partners[openIndex];
        return closeIndex > openIndex;
    }

    public bool IsOpening(int index)
    {
        return partners[index] > index;
    }

    public bool IsClosing(int index)
    {
        return partners[index] >= 0 && partners[index] < index;
    }

    public int GetPartner(int index)
    {
        return partners[index];
    }

    public void Link(int openIndex, int closeIndex)
    {
        partners[openIndex] = closeIndex;
        partners[closeIndex] = openIndex;
    }

    public void Drop(int openIndex)
    {
        var closeIndex = partners[openIndex];
        if (closeIndex < 0)
            return;

        partners[openIndex] = NoPair;
        partners[closeIndex] = NoPair;
    }
}
