namespace Markdown.Pairing;

/// <summary>Пара индексов открывающего и закрывающего разделителей.</summary>
public readonly record struct DelimiterPair(int OpenIndex, int CloseIndex);