namespace Markdown.Pairing;

/// <summary>Группа пар разделителей, снятых вместе одним правилом конфликта.</summary>
public record DroppedConflict(IReadOnlyList<DelimiterPair> Pairs);