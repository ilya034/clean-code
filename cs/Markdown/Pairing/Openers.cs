using Markdown.Tokens;

namespace Markdown.Pairing;

public class Openers
{
    private const int NoOpener = -1;

    private readonly int[] previousVisible;
    private readonly bool[] doomed;
    private readonly Stack<int> emphasis = new();
    private readonly Stack<int> strong = new();
    private int emphasisTop = NoOpener;
    private int strongTop = NoOpener;

    public Openers(int tokenCount)
    {
        previousVisible = new int[tokenCount];
        Array.Fill(previousVisible, NoOpener);
        doomed = new bool[tokenCount];
    }

    public int Count(TokenKind kind)
    {
        return StackFor(kind).Count;
    }

    public int Peek(TokenKind kind)
    {
        return StackFor(kind).Peek();
    }

    public bool IsDoomed(int index)
    {
        return doomed[index];
    }

    /// <summary>Есть ли среди открывашек этого вида видимая — та, у которой впереди закрывашка.</summary>
    public bool HasVisible(TokenKind kind)
    {
        return VisibleTop(kind) != NoOpener;
    }

    public void Push(TokenKind kind, int index, bool willClose)
    {
        StackFor(kind).Push(index);

        if (!willClose)
            return;

        previousVisible[index] = VisibleTop(kind);
        SetVisibleTop(kind, index);
    }

    public void Pop(TokenKind kind)
    {
        var index = StackFor(kind).Pop();
        Hide(kind, index);
    }

    public void ForgetAbove(TokenKind kind, int openIndex)
    {
        while (Count(kind) > 0 && Peek(kind) > openIndex)
            Pop(kind);

        while (VisibleTop(kind) > openIndex)
            SetVisibleTop(kind, previousVisible[VisibleTop(kind)]);
    }

    public bool DoomVisibleAbove(TokenKind kind, int openIndex)
    {
        var doomedAny = false;

        while (VisibleTop(kind) > openIndex)
        {
            var visible = VisibleTop(kind);
            Hide(kind, visible);
            doomed[visible] = true;
            doomedAny = true;
        }

        return doomedAny;
    }

    private Stack<int> StackFor(TokenKind kind)
    {
        return kind == TokenKind.Underscore ? emphasis : strong;
    }

    private int VisibleTop(TokenKind kind)
    {
        return kind == TokenKind.Underscore ? emphasisTop : strongTop;
    }

    private void SetVisibleTop(TokenKind kind, int index)
    {
        if (kind == TokenKind.Underscore)
            emphasisTop = index;
        else
            strongTop = index;
    }

    private void Hide(TokenKind kind, int index)
    {
        if (VisibleTop(kind) == index)
            SetVisibleTop(kind, previousVisible[index]);
    }
}
