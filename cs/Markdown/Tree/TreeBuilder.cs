using Markdown.Pairing;
using Markdown.Syntax;
using Markdown.Tokens;

namespace Markdown.Tree;

public class TreeBuilder
{
    public IReadOnlyList<INode> Build(IReadOnlyList<Token> tokens, TagPairs pairs)
    {
        var root = new List<INode>();
        var frames = new Stack<Frame>();
        frames.Push(Frame.ForRoot(tokens.Count, root));

        while (frames.Count > 0)
        {
            var frame = frames.Peek();

            if (frame.IsFinished)
            {
                frames.Pop();
                CloseOpener(frames, tokens, frame);
                continue;
            }

            var token = tokens[frame.Index];

            if (pairs.TryGetClosingIndex(frame.Index, out var closeIndex))
            {
                var openerIndex = frame.Index;

                frame.Index = GetNextIndexAfter(token, closeIndex);
                frames.Push(Frame.ForOpener(token, openerIndex, closeIndex, new List<INode>()));
                continue;
            }

            frame.Nodes.Add(new TextNode(token.Value));
            frame.Index++;
        }

        return root;
    }

    private static void CloseOpener(Stack<Frame> frames, IReadOnlyList<Token> tokens, Frame finished)
    {
        if (finished.Opener is not { } opener)
            return;

        var parent = frames.Peek();
        parent.Nodes.Add(CreateNode(tokens, opener, finished.End, finished.Nodes));
    }

    private static INode CreateNode(IReadOnlyList<Token> tokens, Token opener, int closeIndex, List<INode> children)
    {
        return opener.Kind switch
        {
            TokenKind.Underscore => new EmNode(children),
            TokenKind.DoubleUnderscore => new StrongNode(children),
            TokenKind.LeftBracket => new LinkNode(ExtractUrl(tokens, closeIndex), children),
            _ => throw new InvalidOperationException($"Открывающей лексемой не может быть {opener.Kind}.")
        };
    }

    private static string ExtractUrl(IReadOnlyList<Token> tokens, int closeBracketIndex)
    {
        var rawUrlWithParens = tokens[closeBracketIndex + 1].Value;
        return rawUrlWithParens[MarkupSymbols.UrlWrapperLength..^MarkupSymbols.UrlWrapperLength];
    }

    private static int GetNextIndexAfter(Token opener, int closeIndex)
    {
        return opener.Kind == TokenKind.LeftBracket
            ? closeIndex + 2
            : closeIndex + 1;
    }

    private sealed class Frame
    {
        private Frame(Token? opener, int index, int end, List<INode> nodes)
        {
            Opener = opener;
            Index = index;
            End = end;
            Nodes = nodes;
        }

        public Token? Opener { get; }

        public int Index { get; set; }

        public int End { get; }

        public List<INode> Nodes { get; }

        public bool IsFinished => Index >= End;

        public static Frame ForRoot(int end, List<INode> nodes)
        {
            return new Frame(null, 0, end, nodes);
        }

        public static Frame ForOpener(Token opener, int openerIndex, int closeIndex, List<INode> nodes)
        {
            return new Frame(opener, openerIndex + 1, closeIndex, nodes);
        }
    }
}
