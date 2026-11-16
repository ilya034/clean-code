using System.Text;
using Markdown.Blocks;
using Markdown.Tree;

namespace Markdown.Rendering;

public class HtmlRenderer
{
    public string Render(BlockKind kind, IReadOnlyList<INode> nodes)
    {
        var tag = TagFor(kind);
        var output = new StringBuilder();

        output.Append(HtmlTags.OpeningTag(tag));
        RenderChildren(nodes, output);
        output.Append(HtmlTags.ClosingTag(tag));

        return output.ToString();
    }

    private static string TagFor(BlockKind kind)
    {
        return kind switch
        {
            BlockKind.Header => HtmlTags.Header,
            BlockKind.Paragraph => HtmlTags.Paragraph,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Неизвестный вид блока.")
        };
    }

    private static void RenderChildren(IReadOnlyList<INode> children, StringBuilder output)
    {
        var pending = new Stack<RenderStep>();

        PushAll(pending, children);

        while (pending.Count > 0)
        {
            var step = pending.Pop();

            if (step.Literal is { } literal)
            {
                output.Append(literal);
                continue;
            }

            switch (step.Node)
            {
                case TextNode text:
                    AppendEscaped(output, text.Text);
                    break;
                case EmNode em:
                    PushContainer(pending, HtmlTags.Emphasis, em.Children);
                    break;
                case StrongNode strong:
                    PushContainer(pending, HtmlTags.Strong, strong.Children);
                    break;
                case LinkNode link:
                    PushLink(pending, link);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(children), step.Node, "Неизвестный узел дерева.");
            }
        }
    }

    private static void PushContainer(Stack<RenderStep> pending, string tag, IReadOnlyList<INode> children)
    {
        pending.Push(RenderStep.ForLiteral(HtmlTags.ClosingTag(tag)));
        PushAll(pending, children);
        pending.Push(RenderStep.ForLiteral(HtmlTags.OpeningTag(tag)));
    }

    private static void PushLink(Stack<RenderStep> pending, LinkNode link)
    {
        pending.Push(RenderStep.ForLiteral(HtmlTags.LinkClose));
        PushAll(pending, link.Children);
        pending.Push(RenderStep.ForLiteral(LinkOpening(link.Url)));
    }

    private static void PushAll(Stack<RenderStep> pending, IReadOnlyList<INode> nodes)
    {
        for (var index = nodes.Count - 1; index >= 0; index--)
            pending.Push(RenderStep.ForNode(nodes[index]));
    }

    private static string LinkOpening(string url)
    {
        var output = new StringBuilder(HtmlTags.LinkOpen);

        AppendEscaped(output, url, escapeQuotes: true);
        output.Append(HtmlTags.LinkOpenEnd);

        return output.ToString();
    }

    private static void AppendEscaped(StringBuilder output, string value, bool escapeQuotes = false)
    {
        foreach (var symbol in value)
            switch (symbol)
            {
                case '&':
                    output.Append(HtmlTags.Ampersand);
                    break;
                case '<':
                    output.Append(HtmlTags.LessThan);
                    break;
                case '>':
                    output.Append(HtmlTags.GreaterThan);
                    break;
                case '"' when escapeQuotes:
                    output.Append(HtmlTags.Quote);
                    break;
                default:
                    output.Append(symbol);
                    break;
            }
    }

    private readonly record struct RenderStep(INode? Node, string? Literal)
    {
        public static RenderStep ForNode(INode node)
        {
            return new RenderStep(node, null);
        }

        public static RenderStep ForLiteral(string literal)
        {
            return new RenderStep(null, literal);
        }
    }
}
