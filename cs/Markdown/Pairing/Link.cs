namespace Markdown.Pairing;

/// <summary>Сложившаяся пара «[текст](url)».</summary>
public readonly record struct Link(int OpenIndex, int CloseIndex);