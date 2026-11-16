namespace Markdown.Blocks;

/// <summary>Вид блока: во что оборачивается абзац целиком.</summary>
public enum BlockKind
{
    /// <summary>Абзац — оборачивается в тег <c>p</c>.</summary>
    Paragraph,

    /// <summary>Заголовок: строка, начавшаяся с <c>"# "</c>, оборачивается в тег <c>h1</c>.</summary>
    Header
}
