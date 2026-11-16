namespace Markdown.Tokens;

/// <summary>Вид лексемы: какой кусок разметки она представляет.</summary>
public enum TokenKind
{
    /// <summary>Обычный текст: в разметке не участвует, попадает в HTML как есть (с экранированием).</summary>
    Text,

    /// <summary>Одиночный подчёрк <c>_</c> — маркер курсива.</summary>
    Underscore,

    /// <summary>Двойной подчёрк <c>__</c> — маркер полужирного.</summary>
    DoubleUnderscore,

    /// <summary>Открывающая скобка <c>[</c> — начало текста ссылки.</summary>
    LeftBracket,

    /// <summary>Закрывающая скобка <c>]</c> — конец текста ссылки.</summary>
    RightBracket,

    /// <summary>
    ///     Часть ссылки в круглых скобках вместе с самими скобками, например <c>(url)</c>.
    ///     Всегда идёт сразу за <see cref="RightBracket" />.
    /// </summary>
    LinkUrl
}
