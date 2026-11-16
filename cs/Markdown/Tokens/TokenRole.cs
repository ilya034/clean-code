namespace Markdown.Tokens;

/// <summary>Роль лексемы: участвует ли она в паре и с какой стороны.</summary>
public enum TokenRole
{
    /// <summary>В разметке не участвует: обычный текст или маркер, которому не нашлось пары.</summary>
    None,

    /// <summary>Открывает пару: курсив, полужирный или текст ссылки.</summary>
    Opening,

    /// <summary>Закрывает пару, открытую парной лексемой с ролью <see cref="Opening" />.</summary>
    Closing
}
